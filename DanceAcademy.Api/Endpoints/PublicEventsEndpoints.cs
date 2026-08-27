#nullable enable
using DanceAcademy.Application.DTOs.Public;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class PublicEventsEndpoints
{
    public static IEndpointRouteBuilder MapPublicEventsEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/public")
            .WithTags("Public - Events");

        group.MapGet("/events", async (AppDbContext db, CancellationToken ct) =>
        {
            var events = await db.Events
                .AsNoTracking()
                .Where(e => e.IsPublished)
                .OrderBy(e => e.EventDate)
                .Select(e => new EventDto(
                    e.Id, e.Title, e.Description, e.Location, e.EventDate, e.Price, e.Capacity,
                    e.Capacity - db.EventRegistrations.Count(r => r.EventId == e.Id),
                    e.ImageUrl))
                .ToListAsync(ct);

            return Results.Ok(events);
        })
        .WithName("PublicGetEvents");

        group.MapGet("/events/{eventId:guid}", async (
            [FromRoute] Guid eventId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var ev = await db.Events.AsNoTracking().SingleOrDefaultAsync(e => e.Id == eventId && e.IsPublished, ct);
            if (ev is null)
                return Results.NotFound(new { message = "Evento no encontrado." });

            var registeredCount = await db.EventRegistrations.AsNoTracking().CountAsync(r => r.EventId == eventId, ct);

            return Results.Ok(new EventDto(ev.Id, ev.Title, ev.Description, ev.Location, ev.EventDate, ev.Price, ev.Capacity, ev.Capacity - registeredCount, ev.ImageUrl));
        })
        .WithName("PublicGetEventDetail");

        return app;
    }
}
