#nullable enable
using DanceAcademy.Application.DTOs.Public;
using DanceAcademy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class PublicLevelsEndpoints
{
    public static IEndpointRouteBuilder MapPublicLevelsEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/public")
            .WithTags("Public - Levels");

        group.MapGet("/levels", async (AppDbContext db, CancellationToken ct) =>
        {
            var levels = await db.Levels
                .AsNoTracking()
                .Where(l => l.IsActive)
                .OrderBy(l => l.Order)
                .Select(l => new LevelDto(l.Id, l.Name))
                .ToListAsync(ct);

            return Results.Ok(levels);
        })
        .WithName("PublicGetLevels");

        return app;
    }
}
