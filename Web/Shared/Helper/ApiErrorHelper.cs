using System;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using API.APIService;

namespace BlazorCocktails.Client.Shared.Helper;

public static class ApiErrorHelper
{
    /// <summary>
    /// Devuelve un mensaje “bonito” y saca banderas útiles.
    /// </summary>
    public static string NiceError(
        Exception ex,
        IStringLocalizer L,
        out int? statusCode,
        out bool authExpired)
    {
        statusCode = null;
        authExpired = false;

        // 1) ProblemDetails tipado (NSwag)
        if (ex is ApiException<ProblemDetails> pdx)
        {
            statusCode = pdx.StatusCode;
            authExpired = pdx.StatusCode == 401;
            return pdx.Result?.Title ?? pdx.Result?.Detail ?? L["Err_Generic"];
        }

        // 2) ApiException genérica: intentamos parsear JSON de respuesta
        if (ex is ApiException ax)
        {
            statusCode = ax.StatusCode;
            authExpired = ax.StatusCode == 401;

            string? msg = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(ax.Response))
                {
                    using var doc = JsonDocument.Parse(ax.Response);
                    var root = doc.RootElement;

                    msg = Try(root, "message")
                       ?? Try(root, "Message")
                       ?? Try(root, "detail")
                       ?? Try(root, "Detail");
                }
            }
            catch { /* ignorar parse errors */ }

            if (!string.IsNullOrWhiteSpace(msg))
                return msg!;

            // fallback por status code
            return ax.StatusCode switch
            {
                400 => L["Err_BadRequest"],
                401 => L["Err_Unauthorized"],
                403 => L["Err_Forbidden"],
                404 => L["Err_NotFound"],
                409 => L["Err_Conflict"],
                _ => L["Err_Generic"]
            };
        }

        // 3) Excepción .NET normal
        return string.Format(L["Common_UnexpectedError"], ex.Message);

        static string? Try(JsonElement e, string name)
            => e.TryGetProperty(name, out var p) ? p.GetString() : null;
    }

    /// <summary>
    /// Maneja el error completo: mensaje, severidad y redirección 401.
    /// </summary>
    public static void Handle(
        Exception ex,
        ISnackbar snackbar,
        IStringLocalizer L,
        NavigationManager nav,
        bool redirectOn401 = true,
        Severity apiSeverity = Severity.Warning,
        Severity defaultSeverity = Severity.Error)
    {
        var msg = NiceError(ex, L, out var status, out var expired);

        var sev = ex is ApiException or ApiException<ProblemDetails>
            ? apiSeverity
            : defaultSeverity;

        snackbar.Add(msg, sev);

        if (redirectOn401 && expired)
        {
            var returnUrl = Uri.EscapeDataString(nav.ToBaseRelativePath(nav.Uri));
            nav.NavigateTo($"/login?returnUrl={returnUrl}", forceLoad: true);
        }
    }
}
