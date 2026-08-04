#nullable enable
using DanceAcademy.Application.DTOs.Admin;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class AdminFaqEndpoints
{
    public static IEndpointRouteBuilder MapAdminFaqEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/admin/faq")
            .WithTags("Admin - Faq")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        group.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
        {
            var items = await db.FaqItems
                .AsNoTracking()
                .OrderBy(f => f.Category).ThenBy(f => f.Order)
                .Select(f => new AdminFaqItemDto(f.Id, f.Question, f.Answer, f.Category, f.Order, f.IsActive))
                .ToListAsync(ct);

            return Results.Ok(items);
        })
        .WithName("AdminGetFaqItems");

        group.MapGet("/{faqItemId:guid}", async (
            [FromRoute] Guid faqItemId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var item = await db.FaqItems.AsNoTracking().SingleOrDefaultAsync(f => f.Id == faqItemId, ct);
            if (item is null)
                return Results.NotFound(new { message = "Pregunta frecuente no encontrada." });

            return Results.Ok(new AdminFaqItemDto(item.Id, item.Question, item.Answer, item.Category, item.Order, item.IsActive));
        })
        .WithName("AdminGetFaqItemDetail");

        group.MapPost("/", async (
            [FromBody] CreateFaqItemRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Question) || string.IsNullOrWhiteSpace(request.Answer) || string.IsNullOrWhiteSpace(request.Category))
                return Results.BadRequest(new { message = "Question, Answer y Category son obligatorios." });

            if (request.Order < 1)
                return Results.BadRequest(new { message = "Order debe ser >= 1." });

            var item = new FaqItem(request.Question, request.Answer, request.Category, request.Order);

            db.FaqItems.Add(item);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/admin/faq/{item.Id}",
                new AdminFaqItemDto(item.Id, item.Question, item.Answer, item.Category, item.Order, item.IsActive));
        })
        .WithName("AdminCreateFaqItem");

        group.MapPut("/{faqItemId:guid}", async (
            [FromRoute] Guid faqItemId,
            [FromBody] UpdateFaqItemRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Question) || string.IsNullOrWhiteSpace(request.Answer) || string.IsNullOrWhiteSpace(request.Category))
                return Results.BadRequest(new { message = "Question, Answer y Category son obligatorios." });

            if (request.Order < 1)
                return Results.BadRequest(new { message = "Order debe ser >= 1." });

            var item = await db.FaqItems.SingleOrDefaultAsync(f => f.Id == faqItemId, ct);
            if (item is null)
                return Results.NotFound(new { message = "Pregunta frecuente no encontrada." });

            item.UpdateDetails(request.Question, request.Answer, request.Category, request.Order);

            if (request.IsActive) item.Activate();
            else item.Deactivate();

            await db.SaveChangesAsync(ct);

            return Results.Ok(new AdminFaqItemDto(item.Id, item.Question, item.Answer, item.Category, item.Order, item.IsActive));
        })
        .WithName("AdminUpdateFaqItem");

        group.MapDelete("/{faqItemId:guid}", async (
            [FromRoute] Guid faqItemId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var item = await db.FaqItems.SingleOrDefaultAsync(f => f.Id == faqItemId, ct);
            if (item is null)
                return Results.NotFound(new { message = "Pregunta frecuente no encontrada." });

            db.FaqItems.Remove(item);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("AdminDeleteFaqItem");

        return app;
    }
}
