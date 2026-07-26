#nullable enable
using DanceAcademy.Application.DTOs;
using DanceAcademy.Application.Interfaces;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
                return Results.Conflict(new { error = "El email ya existe." });

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

        app.MapPost("/auth/forgot-password", async (
            ForgotPasswordRequest request,
            AppDbContext db,
            IEmailService emailService,
            IConfiguration config,
            CancellationToken ct) =>
        {
            var email = request.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
                return Results.BadRequest(new { error = "Email es obligatorio." });

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive, ct);

            // Respuesta genérica aunque el email no exista — evita enumerar usuarios
            if (user is not null)
            {
                var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                    .Replace("+", "-").Replace("/", "_").Replace("=", "");

                user.PasswordResetToken          = resetToken;
                user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
                await db.SaveChangesAsync(ct);

                var publicUrl = config["App:PublicUrl"]?.TrimEnd('/') ?? "https://localhost:7284";
                var resetLink = $"{publicUrl}/reset-password?token={resetToken}";

                await emailService.SendPasswordResetAsync(email, resetLink, ct);
            }

            return Results.Ok(new { message = "Si el email existe, recibirás un enlace para restablecer tu contraseña." });
        });

        app.MapPost("/auth/reset-password", async (
            ResetPasswordRequest request,
            AppDbContext db,
            IPasswordHasher hasher,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return Results.BadRequest(new { error = "Token requerido." });

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
                return Results.BadRequest(new { error = "Contraseña mínima: 8 caracteres." });

            var user = await db.Users.FirstOrDefaultAsync(
                u => u.PasswordResetToken == request.Token && u.IsActive, ct);

            if (user is null || user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
                return Results.BadRequest(new { error = "El enlace es inválido o ha expirado." });

            user.PasswordHash                = hasher.Hash(request.NewPassword);
            user.PasswordResetToken          = null;
            user.PasswordResetTokenExpiresAt = null;
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { message = "Contraseña actualizada correctamente." });
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
