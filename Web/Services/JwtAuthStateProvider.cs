using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using API.APIService;

namespace BlazorCocktails.Client.Services
{
    /// <summary>
    /// AuthenticationStateProvider basado en JWT.
    /// Observa APIClient.Token (evento TokenChanged) y construye el ClaimsPrincipal
    /// a partir del token si existe y no está expirado.
    /// </summary>
    public class JwtAuthStateProvider : AuthenticationStateProvider, IDisposable
    {
        // Estado actual de autenticación (ClaimsPrincipal)
        private ClaimsPrincipal _current = new(new ClaimsIdentity());

        // Reutilizamos el handler para parsear JWT
        private static readonly JwtSecurityTokenHandler _jwtHandler = new();

        public JwtAuthStateProvider()
        {
            _current = BuildPrincipal(APIClient.Token);
            APIClient.TokenChanged += OnTokenChanged;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(_current));

        private void OnTokenChanged()
        {
            _current = BuildPrincipal(APIClient.Token);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_current)));
        }

        public void Dispose() => APIClient.TokenChanged -= OnTokenChanged;

        // Construye el principal desde el JWT (o anónimo si no hay/está inválido/expirado)
        private static ClaimsPrincipal BuildPrincipal(string? jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt))
                return Anonymous();

            try
            {
                var token = _jwtHandler.ReadJwtToken(jwt);

                // Si el token está expirado => anónimo
                var exp = token.Claims.FirstOrDefault(c => c.Type is "exp")?.Value;
                if (exp is not null && IsExpired(exp))
                    return Anonymous();

                // Identidad con TODAS las claims del JWT
                var identity = new ClaimsIdentity(token.Claims, authenticationType: "jwt");
                return new ClaimsPrincipal(identity);
            }
            catch
            {
                return Anonymous();
            }

            static ClaimsPrincipal Anonymous() => new(new ClaimsIdentity());
        }

        // Comprueba expiración a partir del claim "exp" (Unix seconds, UTC)
        private static bool IsExpired(string expClaim)
        {
            if (!long.TryParse(expClaim, out var seconds)) return false;
            var expUtc = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
            return expUtc <= DateTime.UtcNow;
        }
    }
}
