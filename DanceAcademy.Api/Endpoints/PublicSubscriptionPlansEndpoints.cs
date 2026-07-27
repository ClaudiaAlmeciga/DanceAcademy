#nullable enable
using DanceAcademy.Application.DTOs.Public;
using DanceAcademy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class PublicSubscriptionPlansEndpoints
{
    public static IEndpointRouteBuilder MapPublicSubscriptionPlansEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/public")
            .WithTags("Public - SubscriptionPlans");

        group.MapGet("/subscription-plans", async (AppDbContext db, CancellationToken ct) =>
        {
            var plans = await db.SubscriptionPlans
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .Select(p => new SubscriptionPlanDto(p.Id, p.Name, p.Description, p.Price, p.BillingPeriodDays))
                .ToListAsync(ct);

            return Results.Ok(plans);
        })
        .WithName("PublicGetSubscriptionPlans");

        return app;
    }
}
