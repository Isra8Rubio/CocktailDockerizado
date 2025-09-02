using Core.DTO;
using Core.Entities;
using Core.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infraestructura.Data;
using Infraestructura.Repositories;
using Infraestructura.Services;
using Infraestructura.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using RestSharp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Limpiamos los providers por defecto y usamos NLog y httpcontext para capturar datos de usuario
builder.Logging.ClearProviders();
builder.Host.UseNLog();
builder.Services.AddHttpContextAccessor();

// AppConfiguration bind
var appConfig = new Infraestructura.Configuration.AppConfiguration(builder.Configuration);
appConfig.Load();
builder.Services.AddSingleton(appConfig);

// Data & Domain Services
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(
        config.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("Infraestructura")
    )
);

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserService>();

// =======================
// CORS (ajustado para Compose)
// =======================
const string ComposeCors = "ComposeCors";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(ComposeCors, p =>
        p.WithOrigins(
              "http://localhost:8080",
              "https://localhost:8080",
              "http://localhost:7032",   // <- sigues pudiendo probar fuera de Docker
              "https://localhost:7032"
          )
         .AllowAnyHeader()
         .AllowAnyMethod()
    );
});

// HostedService para refrescar cada X segundos
builder.Services.AddHostedService<RandomCocktailHostedService>();
builder.Services.AddScoped<RandomCocktailRepository>();

// External Client
builder.Services.AddSingleton(_ =>
{
    var options = new RestClientOptions("https://www.thecocktaildb.com/api/json/v1/1/")
    {
    };
    return new RestClient(options);
});
builder.Services.AddTransient<CocktailClientService>();

// Identity & Data Protection
builder.Services.AddDataProtection();
builder.Services
    .AddIdentity<Usuario, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT Configuration usando AppConfiguration
var jwtCfg = appConfig.Jwt;
var keyBytes = Encoding.UTF8.GetBytes(jwtCfg.Key!);

// Limpia el mapeo automático de claims
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opts =>
    {
        // 1) Parámetros básicos de validación
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtCfg.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtCfg.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // 2) Evento para comprobar SecurityStamp
        opts.Events = new JwtBearerEvents
        {
            OnTokenValidated = async ctx =>
            {
                var userMgr = ctx.HttpContext.RequestServices
                                      .GetRequiredService<UserManager<Usuario>>();
                var userId = ctx.Principal!.FindFirstValue(ClaimTypes.NameIdentifier);
                var stampToken = ctx.Principal?.FindFirstValue("securityStamp");

                var user = await userMgr.FindByIdAsync(userId!);
                if (user == null || user.SecurityStamp != stampToken)
                {
                    ctx.Fail("Token inválido: SecurityStamp no coincide.");
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("isAdmin", policy =>
        policy.RequireClaim("isAdmin", "true"));

    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddOpenApiDocument(o =>
{
    o.Title = "Cocktails API";
    o.Version = "v1";
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddScoped<IValidator<CredentialsUserDTO>, CredentialsUserDTOValidator>();
builder.Services.AddScoped<IValidator<RegisterUserDTO>, RegisterUserDTOValidator>();
builder.Services.AddTransient<IValidator<ForgotPasswordDTO>, ForgotPasswordDTOValidator>();
builder.Services.AddTransient<IValidator<ResetPasswordDTO>, ResetPasswordDTOValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cocktail API",
        Version = "v1",
        Description = "API Bar"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce:{token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

try
{
    // Crear carpeta de logs si no existe
    Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "logs"));

    // LOG: ver qué cadena de conexión está usando la API
    var effectiveConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "(null)";
    app.Logger.LogInformation("DefaultConnection efectiva: {Conn}", effectiveConn);

    // ==== Preparar Base de Datos (espera + migración/creación segura) ====
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Espera hasta 60s a que SQL esté listo (compose puede tardar)
        for (int i = 1; i <= 30; i++)
        {
            try
            {
                if (db.Database.CanConnect())
                {
                    app.Logger.LogInformation("SQL Server disponible (intento {Attempt}).", i);
                    break;
                }
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex, "SQL aún no disponible (intento {Attempt}).", i);
            }
            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        // OJO: usa las versiones SINCRONAS para evitar dependencias de extensión
        var pending = db.Database.GetPendingMigrations();
        if (pending.Any())
        {
            app.Logger.LogInformation("Aplicando migraciones pendientes...");
            db.Database.Migrate();
        }
        else
        {
            app.Logger.LogInformation("Sin migraciones: creando esquema actual con EnsureCreated()...");
            db.Database.EnsureCreated();
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error preparando la base de datos al inicio.");
    }
    // ==== Fin preparación BD ====

    app.UseRouting();

    // --- Swagger por defecto (Swashbuckle) ---
    app.UseSwagger();
    app.UseSwaggerUI();

    // *** CORS para Compose ***
    app.UseCors(ComposeCors);

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    // En caso de fallo en el arranque
    var logger = LogManager.GetCurrentClassLogger();
    logger.Error(ex, "Se produjo un error al arrancar la aplicación");
    throw;
}
finally
{
    // Fuerza a NLog a cerrar y vaciar todos los targets
    LogManager.Shutdown();
}


//// ==== Seed de usuario/rol admin (solo dev) ====
//try
//{
//    using var scope = app.Services.CreateScope();
//    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
//    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

//    // Puedes sobreescribir por variables de entorno Admin__Email / Admin__Password
//    var adminEmail = builder.Configuration["Admin:Email"] ?? "admin@gmail.com";
//    var adminPass = builder.Configuration["Admin:Password"] ?? "aA123456!";

//    // Rol Admin
//    const string adminRole = "Admin";
//    if (!await roleMgr.RoleExistsAsync(adminRole))
//        await roleMgr.CreateAsync(new IdentityRole(adminRole));

//    // Usuario admin
//    var admin = await userMgr.FindByEmailAsync(adminEmail);
//    if (admin is null)
//    {
//        admin = new Usuario
//        {
//            UserName = adminEmail,
//            Email = adminEmail,
//            EmailConfirmed = true,
//            SecurityStamp = Guid.NewGuid().ToString()
//        };

//        var res = await userMgr.CreateAsync(admin, adminPass);
//        if (!res.Succeeded)
//        {
//            var errs = string.Join("; ", res.Errors.Select(e => e.Description));
//            app.Logger.LogError("No se pudo crear el usuario admin: {Errors}", errs);
//        }
//    }

//    // Asegura rol + claim isAdmin
//    if (!await userMgr.IsInRoleAsync(admin, adminRole))
//        await userMgr.AddToRoleAsync(admin, adminRole);

//    var claims = await userMgr.GetClaimsAsync(admin);
//    if (!claims.Any(c => c.Type == "isAdmin" && c.Value == "true"))
//        await userMgr.AddClaimAsync(admin, new Claim("isAdmin", "true"));

//    app.Logger.LogInformation("Seed admin OK: {Email}", adminEmail);
//}
//catch (Exception ex)
//{
//    app.Logger.LogError(ex, "Error sembrando usuario admin.");
//}
//// ==== Fin seed admin ====