using Core.DTO;
using Core.Entities;
using Infraestructura.Configuration;
using Infraestructura.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructura.Services
{
    public class UserService
    {
        private readonly UserRepository _repo;
        private readonly AppConfiguration _config;

        public UserService(UserRepository repo, AppConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        public Task<int> CountAsync(CancellationToken ct = default) => _repo.CountAsync(ct);

        public async Task<Usuario?> ResolveUserAsync(string userOrEmail)
        {
            try
            {
                return await _repo.FindByNameOrEmailAsync(userOrEmail);
            }
            catch (Exception ex)
            {
                throw new Exception("UserService.ResolveUserAsync error", ex);
            }
        }


        public async Task<AnswerAuthenticationDTO> RegisterAsync(RegisterUserDTO dto)
        {
            try
            {
                var user = new Usuario { UserName = dto.Email, Email = dto.Email };
                var result = await _repo.CreateUserAsync(user, dto.Password!);
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
                return await BuildTokenAsync(dto.Email!);
            }
            catch (Exception ex)
            {
                throw new Exception("UserService.RegisterAsync error", ex);
            }
        }

        public async Task<AnswerAuthenticationDTO> LoginAsync(CredentialsUserDTO creds)
        {
            try
            {
                var result = await _repo.PasswordSignInAsync(creds.Email!, creds.Password!);
                if (!result.Succeeded)
                    throw new UnauthorizedAccessException();
                return await BuildTokenAsync(creds.Email!);
            }
            catch (Exception ex)
            {
                throw new Exception("UserService.LoginAsync error", ex);
            }
        }

        public IEnumerable<UserDTO> GetAllUsers()
        {
            try
            {
                return _repo.GetAllUsers()
                    .Select(u => new UserDTO { Id = u.Id, Email = u.Email! })
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("UserService.GetAllUsers error", ex);
            }
        }

        public async Task ChangeOwnPasswordAsync(string userId, ChangePasswordDTO dto)
        {
            try
            {
                var user = await _repo.FindByIdAsync(userId)
                           ?? throw new KeyNotFoundException("Usuario no encontrado.");
                var result = await _repo.ChangePasswordAsync(
                    user,
                    dto.CurrentPassword ?? "",
                    dto.NewPassword ?? "");
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException(errors);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("UserService.ChangeOwnPasswordAsync error", ex);
            }
        }

        public async Task DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _repo.FindByIdAsync(userId)
                           ?? throw new KeyNotFoundException();
                var result = await _repo.DeleteUserAsync(user);
                if (!result.Succeeded)
                    throw new InvalidOperationException(
                        string.Join("; ", result.Errors.Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                throw new Exception("UserService.DeleteUserAsync error", ex);
            }
        }

        public async Task AssignAdminAsync(string email)
        {
            try
            {
                var user = await _repo.FindByEmailAsync(email)
                           ?? throw new KeyNotFoundException();
                var claims = await _repo.GetClaimsAsync(user);
                if (claims.Any(c => c.Type == "isAdmin" && c.Value == "true"))
                    throw new InvalidOperationException();
                var result = await _repo.AddClaimAsync(user, new Claim("isAdmin", "true"));
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
            }
            catch (Exception ex)
            {
                throw new Exception("UserService.AssignAdminAsync error", ex);
            }
        }

        private async Task<AnswerAuthenticationDTO> BuildTokenAsync(string email)
        {
            try
            {
                // Busca al usuario por email
                var user = await _repo.FindByEmailAsync(email)
                           ?? throw new KeyNotFoundException("User not found.");

                // Creamos los claims idénticos a antes
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id!),
            new Claim(ClaimTypes.Email, email),
            new Claim("securityStamp", user.SecurityStamp!)
        };
                claims.AddRange(await _repo.GetClaimsAsync(user));
                if ((await _repo.GetRolesAsync(user)).Contains("Admin"))
                    claims.Add(new Claim("isAdmin", "true"));

                // Generamos el token igual que antes
                var jwtCfg = _config.Jwt;
                var keyBytes = Encoding.UTF8.GetBytes(jwtCfg.Key!);
                var signingKey = new SymmetricSecurityKey(keyBytes);
                var credsSign = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
                var expires = DateTime.UtcNow.AddMinutes(jwtCfg.ExpiresInMinutes);
                var token = new JwtSecurityToken(
                    issuer: jwtCfg.Issuer,
                    audience: jwtCfg.Audience,
                    claims: claims,
                    expires: expires,
                    signingCredentials: credsSign
                );

                return new AnswerAuthenticationDTO
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = expires
                };
            }
            catch (Exception ex)
            {
                throw new Exception("UserService.BuildTokenAsync error", ex);
            }
        }

        public async Task ForgotPasswordAsync(string email, string clientResetBaseUrl)
        {
            var user = await _repo.FindByEmailAsync(email);
            if (user is null) return;

            var token = await _repo.GeneratePasswordResetTokenAsync(user);
            var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{clientResetBaseUrl}?email={Uri.EscapeDataString(email)}&token={tokenEncoded}";

            var html = $@"<p>Para restablecer tu contraseña, haz clic:</p>
                      <p><a href=""{url}"">Restablecer contraseña</a></p>
                      <p>Si no lo solicitaste, ignora este mensaje.</p>";

            await SendEmailAsync(email, "Restablecer contraseña", html);
        }

        public async Task ResetPasswordAsync(ResetPasswordDTO dto)
        {
            var user = await _repo.FindByEmailAsync(dto.Email ?? "")
                       ?? throw new KeyNotFoundException("Usuario no encontrado");
            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token ?? ""));
            var result = await _repo.ResetPasswordAsync(user, token, dto.NewPassword ?? "");
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        private async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var e = _config.Email;

            using var client = new SmtpClient(e.Host, e.Port)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = e.EnableSsl,
                Timeout = 10000
            };

            using var msg = new MailMessage
            {
                From = new MailAddress(string.IsNullOrWhiteSpace(e.From) ? "dev@local" : e.From, e.FromName ?? "Dev"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            msg.To.Add(string.IsNullOrWhiteSpace(to) ? "test@local" : to);

            await client.SendMailAsync(msg);
        }

    }
}

