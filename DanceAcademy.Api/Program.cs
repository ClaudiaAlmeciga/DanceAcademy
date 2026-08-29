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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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

// Seed Admin
await app.SeedAdminAsync();

app.Run();