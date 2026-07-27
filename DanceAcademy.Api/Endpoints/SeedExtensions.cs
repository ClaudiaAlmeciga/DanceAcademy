#nullable enable
using DanceAcademy.Application.Interfaces;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class SeedExtensions
{
    public static async Task SeedAdminAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");

        var adminEmail = (config["Seed:AdminEmail"] ?? "admin@danceacademy.local").Trim();
        var adminPassword = config["Seed:AdminPassword"] ?? "Admin12345!";

        // Idempotente: si existe, no crea duplicados
        var exists = await db.Users.AnyAsync(u => u.Email == adminEmail);
        if (exists)
        {
            logger.LogInformation("Seed Admin: ya existe {Email}", adminEmail);
            return;
        }

        var admin = new User
        {
            Email = adminEmail,
            PasswordHash = hasher.Hash(adminPassword),
            Role = Roles.Admin,
            IsActive = true
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();

        logger.LogInformation("Seed Admin: creado {Email}", adminEmail);
    }
}