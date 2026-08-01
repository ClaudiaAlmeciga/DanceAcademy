#nullable enable
using DanceAcademy.Application.DTOs.Public;
using DanceAcademy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class PublicFaqEndpoints
{
    public static IEndpointRouteBuilder MapPublicFaqEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/public")
            .WithTags("Public - Faq");

        group.MapGet("/faq", async (AppDbContext db, CancellationToken ct) =>
        {
            var items = await db.FaqItems
                .AsNoTracking()
                .Where(f => f.IsActive)
                .OrderBy(f => f.Category).ThenBy(f => f.Order)
                .Select(f => new FaqItemDto(f.Id, f.Question, f.Answer, f.Category))
                .ToListAsync(ct);

            return Results.Ok(items);
        })
        .WithName("PublicGetFaqItems");

        return app;
    }
}
