#nullable enable
using DanceAcademy.Api.Endpoints;
using DanceAcademy.Infrastructure;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// CORS — permite que los clientes Blazor WASM consuman la API.
// Los orígenes de localhost siempre se permiten (desarrollo). Los de producción se leen de
// configuración (Cors:AllowedOrigins, ej. variables de entorno Cors__AllowedOrigins__0/__1)
// en vez de quedar fijos en el código — así un cambio de dominio no requiere tocar el código.
var corsOrigins = new List<string>
{
    "http://localhost:5241", "https://localhost:7282", // Admin (dev)
    "http://localhost:5182", "https://localhost:7284"  // Public (dev)
};
corsOrigins.AddRange(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? []);

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClients", policy =>
        policy.WithOrigins(corsOrigins.ToArray())
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// DB
var cs = builder.Configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(cs))
    throw new InvalidOperationException("Falta ConnectionStrings:Default en appsettings.json.");

// Proveedores como Render entregan la cadena de conexión de Postgres en formato URI
// (postgres://usuario:clave@host:puerto/basededatos), pero Npgsql espera el formato
// ADO.NET (Host=...;Port=...;Database=...). Se convierte automáticamente si hace falta,
// así el mismo appsettings/user-secrets local (ADO.NET) y el de producción (URI) funcionan
// sin tocar código.
if (cs.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
    cs.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    var uri = new Uri(cs);
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    var port = uri.Port > 0 ? uri.Port : 5432;

    cs = $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')};" +
         $"Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(cs));

// DI
builder.Services.AddInfrastructure();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Definici�n de seguridad: Bearer JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token as�: Bearer {token}"
    });

    // Requisito global (para que Swagger muestre el candado y el bot�n Authorize)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Falta Jwt:Key en appsettings.json.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger: en Development queda abierto (como siempre). Fuera de Development (ej. Render)
// se protege con Basic Auth — la página de Swagger se navega directo en el browser, sin
// token JWT adjunto, así que [Authorize] normal no aplica; Basic Auth es el estándar para
// este caso. Sin Swagger:Username/Swagger:Password configurados, queda oculto (404).
app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/swagger") || app.Environment.IsDevelopment())
    {
        await next();
        return;
    }

    var swaggerUser = app.Configuration["Swagger:Username"];
    var swaggerPassword = app.Configuration["Swagger:Password"];

    if (string.IsNullOrWhiteSpace(swaggerUser) || string.IsNullOrWhiteSpace(swaggerPassword))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    var header = context.Request.Headers.Authorization.ToString();
    if (header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
    {
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header["Basic ".Length..]));
        var parts = decoded.Split(':', 2);
        if (parts.Length == 2 && parts[0] == swaggerUser && parts[1] == swaggerPassword)
        {
            await next();
            return;
        }
    }

    context.Response.Headers.WWWAuthenticate = "Basic realm=\"Swagger\"";
    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("WebClients");
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapAuthEndpoints();
app.MapMeEndpoints();
app.MapAdminEndpoints();
app.MapAdminCoursesEndpoints();
app.MapPublicCoursesEndpoints();
app.MapAdminLevelsEndpoints();
app.MapPublicLevelsEndpoints();
app.MapAdminSubscriptionPlansEndpoints();
app.MapPublicSubscriptionPlansEndpoints();
app.MapMeEnrollmentsEndpoints();
app.MapMeProgressEndpoints();
app.MapAdminDashboardEndpoints();
app.MapMeDashboardEndpoints();
app.MapAdminInstructorsEndpoints();
app.MapPublicInstructorsEndpoints();
app.MapAdminTestimonialsEndpoints();
app.MapPublicTestimonialsEndpoints();
app.MapAdminFaqEndpoints();
app.MapPublicFaqEndpoints();
app.MapMeCertificatesEndpoints();
app.MapAdminStudentsEndpoints();
app.MapMeTestimonialsEndpoints();
app.MapAdminEventsEndpoints();
app.MapPublicEventsEndpoints();
app.MapMeEventRegistrationsEndpoints();
app.MapAdminNewsEndpoints();
app.MapPublicNewsEndpoints();

// Aplica migraciones pendientes al arrancar. En local ya se aplican a mano con
// "dotnet ef database update", pero en un despliegue nuevo (ej. Render, con una base
// de datos recién creada) no hay ningún paso separado que lo haga — sin esto, la base
// queda sin tablas y el seed de abajo (y cualquier request) falla.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// Seed Admin
await app.SeedAdminAsync();

app.Run();