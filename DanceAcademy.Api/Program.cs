#nullable enable
using System.Text;
using DanceAcademy.Api.Endpoints;
using DanceAcademy.Infrastructure;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// DB
var cs = builder.Configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(cs))
    throw new InvalidOperationException("Falta ConnectionStrings:Default en appsettings.json.");

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(cs));

// DI
builder.Services.AddInfrastructure();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapAuthEndpoints();
app.MapMeAndAdmin();

// Seed Admin
await app.SeedAdminAsync();

app.Run();