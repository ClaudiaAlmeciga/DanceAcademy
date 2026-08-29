#nullable enable
using DanceAcademy.Application.DTOs.Admin;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class AdminEventsEndpoints
{
    public static IEndpointRouteBuilder MapAdminEventsEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/admin/events")
            .WithTags("Admin - Events")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        group.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
        {
            var events = await db.Events
                .AsNoTracking()
                .OrderByDescending(e => e.EventDate)
                .Select(e => new AdminEventDto(
                    e.Id, e.Title, e.Description, e.Location, e.EventDate, e.Price, e.Capacity,
                    db.EventRegistrations.Count(r => r.EventId == e.Id),
                    e.ImageUrl,
                    e.IsPublished))
                .ToListAsync(ct);

            return Results.Ok(events);
        })
        .WithName("AdminGetEvents");

        group.MapGet("/{eventId:guid}", async (
            [FromRoute] Guid eventId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var ev = await db.Events.AsNoTracking().SingleOrDefaultAsync(e => e.Id == eventId, ct);
            if (ev is null)
                return Results.NotFound(new { message = "Evento no encontrado." });

            var registeredCount = await db.EventRegistrations.AsNoTracking().CountAsync(r => r.EventId == eventId, ct);

            return Results.Ok(new AdminEventDto(ev.Id, ev.Title, ev.Description, ev.Location, ev.EventDate, ev.Price, ev.Capacity, registeredCount, ev.ImageUrl, ev.IsPublished));
        })
        .WithName("AdminGetEventDetail");

        group.MapPost("/", async (
            [FromBody] CreateEventRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Title))
                return Results.BadRequest(new { message = "Title es obligatorio." });

            Event ev;
            try
            {
                ev = new Event(request.Title, request.Description, request.Location, request.EventDate, request.Price, request.Capacity, request.ImageUrl);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }

            db.Events.Add(ev);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/admin/events/{ev.Id}",
                new AdminEventDto(ev.Id, ev.Title, ev.Description, ev.Location, ev.EventDate, ev.Price, ev.Capacity, 0, ev.ImageUrl, ev.IsPublished));
        })
        .WithName("AdminCreateEvent");

        group.MapPut("/{eventId:guid}", async (
            [FromRoute] Guid eventId,
            [FromBody] UpdateEventRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Title))
                return Results.BadRequest(new { message = "Title es obligatorio." });

            var ev = await db.Events.SingleOrDefaultAsync(e => e.Id == eventId, ct);
            if (ev is null)
                return Results.NotFound(new { message = "Evento no encontrado." });

            var registeredCount = await db.EventRegistrations.AsNoTracking().CountAsync(r => r.EventId == eventId, ct);

            // El cupo no puede bajar por debajo de las inscripciones ya existentes —
            // de lo contrario "cupos disponibles" en el sitio público quedaría negativo.
            if (request.Capacity < registeredCount)
                return Results.BadRequest(new { message = $"El cupo no puede ser menor que las {registeredCount} inscripciones ya registradas." });

            try
            {
                ev.UpdateDetails(request.Title, request.Description, request.Location, request.EventDate, request.Price, request.Capacity, request.ImageUrl);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }

            await db.SaveChangesAsync(ct);

            return Results.Ok(new AdminEventDto(ev.Id, ev.Title, ev.Description, ev.Location, ev.EventDate, ev.Price, ev.Capacity, registeredCount, ev.ImageUrl, ev.IsPublished));
        })
        .WithName("AdminUpdateEvent");

        group.MapDelete("/{eventId:guid}", async (
            [FromRoute] Guid eventId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var ev = await db.Events.SingleOrDefaultAsync(e => e.Id == eventId, ct);
            if (ev is null)
                return Results.NotFound(new { message = "Evento no encontrado." });

            db.Events.Remove(ev);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("AdminDeleteEvent");

        group.MapPatch("/{eventId:guid}/publish", async (
            [FromRoute] Guid eventId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var ev = await db.Events.SingleOrDefaultAsync(e => e.Id == eventId, ct);
            if (ev is null)
                return Results.NotFound(new { message = "Evento no encontrado." });

            ev.Publish();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { ev.Id, ev.IsPublished });
        })
        .WithName("AdminPublishEvent");

        group.MapPatch("/{eventId:guid}/unpublish", async (
            [FromRoute] Guid eventId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var ev = await db.Events.SingleOrDefaultAsync(e => e.Id == eventId, ct);
            if (ev is null)
                return Results.NotFound(new { message = "Evento no encontrado." });

            ev.Unpublish();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { ev.Id, ev.IsPublished });
        })
        .WithName("AdminUnpublishEvent");

        // GET /{eventId}/registrations — inscritos al evento, para gestionar el pago manualmente
        group.MapGet("/{eventId:guid}/registrations", async (
            [FromRoute] Guid eventId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var eventExists = await db.Events.AsNoTracking().AnyAsync(e => e.Id == eventId, ct);
            if (!eventExists)
                return Results.NotFound(new { message = "Evento no encontrado." });

            var registrations = await (
                from r in db.EventRegistrations.AsNoTracking()
                join u in db.Users.AsNoTracking() on r.UserId equals u.Id
                where r.EventId == eventId
                orderby r.RegisteredAt
                select new AdminEventRegistrationDto(r.Id, r.UserId, u.Email, u.FullName, r.Status, r.RegisteredAt, r.PaidAt))
                .ToListAsync(ct);

            return Results.Ok(registrations);
        })
        .WithName("AdminGetEventRegistrations");

        // PATCH /{eventId}/registrations/{registrationId}/mark-paid — la pasarela de pago (Wompi)
        // todavía no está conectada para eventos, así que el Admin confirma el pago manualmente.
        group.MapPatch("/{eventId:guid}/registrations/{registrationId:guid}/mark-paid", async (
            [FromRoute] Guid eventId,
            [FromRoute] Guid registrationId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var registration = await db.EventRegistrations.SingleOrDefaultAsync(r => r.Id == registrationId && r.EventId == eventId, ct);
            if (registration is null)
                return Results.NotFound(new { message = "Inscripción no encontrada." });

            registration.MarkPaid();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { registration.Id, registration.Status, registration.PaidAt });
        })
        .WithName("AdminMarkEventRegistrationPaid");

        return app;
    }
}
