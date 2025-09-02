using API.APIService;
using BlazorCocktails.Client;
using BlazorCocktails.Client.Services;
using BlazorCocktails.Client.Shared.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");

builder.RootComponents.Add<HeadOutlet>("head::after");

// -------------------------------------
// UI
// -------------------------------------
builder.Services.AddMudServices();

// -------------------------------------
// AuthN / AuthZ
// -------------------------------------
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("isAdmin", policy => policy.RequireClaim("isAdmin", "true"));
});
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();

// -------------------------------------
// HTTP / API client (typed) + redirect 401/403
// -------------------------------------
builder.Services.AddTransient<AuthRedirectHandler>();

// Si se sirve desde http://localhost:<puerto> (Docker/IIS Express),
// usamos la API publicada en http://localhost:8081.
// Si no, mantenemos tu API de dev en https://localhost:7131.
var baseUri = new Uri(builder.HostEnvironment.BaseAddress);
bool isLocalHttpHost = baseUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                       && baseUri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase);
var apiBaseUrl = isLocalHttpHost ? "http://localhost:8081" : "https://localhost:7131";

builder.Services.AddHttpClient<APIClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthRedirectHandler>();

// -------------------------------------
// Validación
// -------------------------------------
builder.Services.AddValidatorsFromAssemblyContaining<CredentialsUserDTOValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ForgotPasswordDTOValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ResetModelValidator>();

// -------------------------------------
// Localización
// -------------------------------------
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

// -------------------------------------
// Build + cultura almacenada
// -------------------------------------
var host = builder.Build();

var js = host.Services.GetRequiredService<IJSRuntime>();
var stored = await js.InvokeAsync<string?>("blazorCulture.get");
var culture = !string.IsNullOrWhiteSpace(stored) ? new CultureInfo(stored!) : new CultureInfo("es-ES");

CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await host.RunAsync();
