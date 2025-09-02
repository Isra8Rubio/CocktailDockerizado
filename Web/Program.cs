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

builder.Services.AddHttpClient<APIClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7131");
})
.AddHttpMessageHandler<AuthRedirectHandler>();

// -------------------------------------F
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
