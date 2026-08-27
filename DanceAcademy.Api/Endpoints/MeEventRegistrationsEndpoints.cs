#nullable enable
using DanceAcademy.Application.DTOs;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DanceAcademy.Api.Endpoints;

public static class MeEventRegistrationsEndpoints
{
    public static IEndpointRouteBuilder MapMeEventRegistrationsEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/me/event-registrations")
            .WithTags("Me - Event Registrations")
            .RequireAuthorization();

        group.MapGet("/", async (
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var registrations = await (
                from r in db.EventRegistrations.AsNoTracking()
                join e in db.Events.AsNoTracking() on r.EventId equals e.Id
                where r.UserId == userId
                orderby e.EventDate
                select new MyEventRegistrationDto(r.Id, e.Id, e.Title, e.EventDate, e.Location, e.Price, r.Status, r.RegisteredAt, e.ImageUrl))
                .ToListAsync(ct);

            return Results.Ok(registrations);
        })
        .WithName("GetMyEventRegistrations");

        group.MapPost("/", async (
            [FromBody] CreateEventRegistrationRequest request,
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (request is null || request.EventId == Guid.Empty)
                return Results.BadRequest(new { message = "EventId es obligatorio." });

            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var ev = await db.Events.AsNoTracking().SingleOrDefaultAsync(e => e.Id == request.EventId, ct);
            if (ev is null || !ev.IsPublished)
                return Results.NotFound(new { message = "Evento no encontrado." });

            var isAlreadyRegistered = await db.EventRegistrations
                .AsNoTracking()
                .AnyAsync(r => r.UserId == userId && r.EventId == request.EventId, ct);
            if (isAlreadyRegistered)
                return Results.Conflict(new { message = "Ya estás inscrito en este evento." });

            var registeredCount = await db.EventRegistrations.AsNoTracking().CountAsync(r => r.EventId == request.EventId, ct);
            if (registeredCount >= ev.Capacity)
                return Results.Conflict(new { message = "Este evento ya no tiene cupo disponible." });

            var registration = new EventRegistration(userId, request.EventId);

            db.EventRegistrations.Add(registration);
            await db.SaveChangesAsync(ct);

            var dto = new MyEventRegistrationDto(registration.Id, ev.Id, ev.Title, ev.EventDate, ev.Location, ev.Price, registration.Status, registration.RegisteredAt, ev.ImageUrl);
            return Results.Created($"/me/event-registrations/{registration.Id}", dto);
        })
        .WithName("CreateMyEventRegistration");

        return app;
    }
}
