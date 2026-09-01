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

        // Normalizado a minúsculas — igual que en /auth/register y /auth/login — para que el
        // admin sembrado pueda iniciar sesión sin importar cómo se haya escrito el email en
        // la configuración.
        var adminEmail = config["Seed:AdminEmail"]?.Trim().ToLowerInvariant();
        var adminPassword = config["Seed:AdminPassword"];

        // Sin credenciales configuradas (Seed:AdminEmail / Seed:AdminPassword), no se crea ningún
        // admin. Antes había un fallback fijo ("Admin12345!") — un default público y adivinable es
        // peor que no sembrar nada.
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("Seed Admin: Seed:AdminEmail / Seed:AdminPassword no configurados. No se creó ningún admin.");
            return;
        }

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