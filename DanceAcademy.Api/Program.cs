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

// CORS — permite que los clientes Blazor WASM consuman la API
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClients", policy =>
        policy.WithOrigins(
                  "http://localhost:5241",  "https://localhost:7282",  // Admin
                  "http://localhost:5182",  "https://localhost:7284"   // Public
              )
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

// Seed Admin
await app.SeedAdminAsync();

app.Run();