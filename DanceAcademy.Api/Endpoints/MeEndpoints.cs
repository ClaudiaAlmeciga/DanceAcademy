#nullable enable
using DanceAcademy.Application.DTOs;
using DanceAcademy.Application.Interfaces;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DanceAcademy.Api.Endpoints;

public static class MeEndpoints
{
    public static void MapMeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me", [Authorize] async (
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var profile = await db.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new MeProfileDto(u.Id, u.Email, u.Role, u.FullName, u.Phone, u.BirthDate))
                .SingleOrDefaultAsync(ct);

            return profile is null
                ? Results.NotFound(new { message = "Usuario no encontrado." })
                : Results.Ok(profile);
        });

        app.MapPut("/me", [Authorize] async (
            [FromBody] UpdateProfileRequest request,
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
                return Results.NotFound(new { message = "Usuario no encontrado." });

            user.UpdateProfile(request.FullName, request.Phone, request.BirthDate);
            await db.SaveChangesAsync(ct);

            var profile = new MeProfileDto(user.Id, user.Email, user.Role, user.FullName, user.Phone, user.BirthDate);
            return Results.Ok(profile);
        });

        app.MapPut("/me/password", [Authorize] async (
            [FromBody] ChangePasswordRequest request,
            ClaimsPrincipal principal,
            AppDbContext db,
            IPasswordHasher passwordHasher,
            CancellationToken ct) =>
        {
            var currentPassword = request.CurrentPassword;
            var newPassword = request.NewPassword;

            if (string.IsNullOrWhiteSpace(currentPassword))
                return Results.BadRequest(new { error = "La contraseña actual es obligatoria." });

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
                return Results.BadRequest(new { error = "La nueva contraseña debe tener al menos 8 caracteres." });

            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
                return Results.NotFound(new { message = "Usuario no encontrado." });

            var isCurrentPasswordValid = passwordHasher.Verify(currentPassword, user.PasswordHash);
            if (!isCurrentPasswordValid)
                return Results.BadRequest(new { error = "La contraseña actual es incorrecta." });

            user.PasswordHash = passwordHasher.Hash(newPassword);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { message = "Contraseña actualizada correctamente." });
        });
    }
}
