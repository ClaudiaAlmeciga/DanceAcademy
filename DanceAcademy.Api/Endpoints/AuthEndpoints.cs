#nullable enable
using DanceAcademy.Application.DTOs;
using DanceAcademy.Application.Interfaces;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DanceAcademy.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (
            RegisterUserRequest request,
            AppDbContext db,
            IPasswordHasher hasher,
            CancellationToken ct) =>
        {
            if (request is null)
                return Results.BadRequest(new { error = "Body requerido." });

            var email = request.Email?.Trim();
            var password = request.Password;

            if (string.IsNullOrWhiteSpace(email))
                return Results.BadRequest(new { error = "Email es obligatorio." });

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return Results.BadRequest(new { error = "Contraseña mínima: 8 caracteres." });

            var exists = await db.Users.AsNoTracking().AnyAsync(u => u.Email == email, ct);
            if (exists)
                return Results.BadRequest(new { error = "El email ya existe." });

            var user = new User
            {
                Email = email,
                PasswordHash = hasher.Hash(password),
                Role = Roles.Student
            };

            db.Users.Add(user);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { message = "Usuario creado." });
        });

        app.MapPost("/auth/login", async (
            LoginUserRequest request,
            AppDbContext db,
            IPasswordHasher hasher,
            IConfiguration config,
            CancellationToken ct) =>
        {
            if (request is null)
                return Results.BadRequest(new { error = "Body requerido." });

            var email = request.Email?.Trim();
            var password = request.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return Results.BadRequest(new { error = "Email y contraseña son obligatorios." });

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive, ct);
            if (user is null || !hasher.Verify(password, user.PasswordHash))
                return Results.Unauthorized();

            var jwtKey = config["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
                return Results.Problem("Falta configuración Jwt:Key.");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return Results.Ok(new { access_token = new JwtSecurityTokenHandler().WriteToken(token) });
        });
    }
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Student = "Student";
}
