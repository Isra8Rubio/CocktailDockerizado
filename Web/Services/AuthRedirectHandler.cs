using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorCocktails.Client.Services
{
    /// <summary>
    /// Handler de HttpClient que intercepta respuestas 401/403.
    /// Limpia el token, resetea APIClient.Token y redirige a /login con returnUrl.
    /// </summary>
    public sealed class AuthRedirectHandler : DelegatingHandler
    {
        private readonly NavigationManager _nav;
        private readonly IJSRuntime _js;

        private const string TokenKey = "authToken";

        public AuthRedirectHandler(NavigationManager nav, IJSRuntime js)
        {
            _nav = nav;
            _js = js;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                // 1) Borrar token en storage
                try
                {
                    await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
                    await _js.InvokeVoidAsync("sessionStorage.removeItem", TokenKey);
                }
                catch
                {
                    // Ignorar errores de JS interop
                }

                // 2) Resetear token del cliente generado
                API.APIService.APIClient.Token = null;

                // 3) Construir returnUrl y redirigir a /login
                var current = _nav.ToBaseRelativePath(_nav.Uri);
                var returnUrl = string.IsNullOrWhiteSpace(current)
                    ? string.Empty
                    : $"?returnUrl=/{Uri.EscapeDataString(current)}";

                _nav.NavigateTo($"/login{returnUrl}", forceLoad: true);
            }

            return response;
        }
    }
}
