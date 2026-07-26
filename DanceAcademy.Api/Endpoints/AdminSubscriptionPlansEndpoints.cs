#nullable enable
using DanceAcademy.Application.DTOs.Admin;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class AdminSubscriptionPlansEndpoints
{
    public static IEndpointRouteBuilder MapAdminSubscriptionPlansEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/admin/subscription-plans")
            .WithTags("Admin - SubscriptionPlans")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        // GET /admin/subscription-plans
        group.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
        {
            var plans = await db.SubscriptionPlans
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new AdminSubscriptionPlanDto(p.Id, p.Name, p.Description, p.Price, p.BillingPeriodDays, p.IsActive))
                .ToListAsync(ct);

            return Results.Ok(plans);
        })
        .WithName("AdminGetSubscriptionPlans");

        // GET /admin/subscription-plans/{planId}
        group.MapGet("/{planId:guid}", async (
            [FromRoute] Guid planId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var plan = await db.SubscriptionPlans.AsNoTracking().SingleOrDefaultAsync(p => p.Id == planId, ct);
            if (plan is null)
                return Results.NotFound(new { message = "Plan no encontrado." });

            return Results.Ok(new AdminSubscriptionPlanDto(plan.Id, plan.Name, plan.Description, plan.Price, plan.BillingPeriodDays, plan.IsActive));
        })
        .WithName("AdminGetSubscriptionPlanDetail");

        // POST /admin/subscription-plans
        group.MapPost("/", async (
            [FromBody] CreateSubscriptionPlanRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { message = "Name es obligatorio." });

            var name = request.Name.Trim();
            var nameExists = await db.SubscriptionPlans.AsNoTracking().AnyAsync(p => p.Name == name, ct);
            if (nameExists)
                return Results.Conflict(new { message = "Ya existe un plan con ese nombre." });

            SubscriptionPlan plan;
            try
            {
                plan = new SubscriptionPlan(name, request.Description, request.Price, request.BillingPeriodDays);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }

            db.SubscriptionPlans.Add(plan);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/admin/subscription-plans/{plan.Id}",
                new AdminSubscriptionPlanDto(plan.Id, plan.Name, plan.Description, plan.Price, plan.BillingPeriodDays, plan.IsActive));
        })
        .WithName("AdminCreateSubscriptionPlan");

        // PUT /admin/subscription-plans/{planId}
        group.MapPut("/{planId:guid}", async (
            [FromRoute] Guid planId,
            [FromBody] UpdateSubscriptionPlanRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { message = "Name es obligatorio." });

            var plan = await db.SubscriptionPlans.SingleOrDefaultAsync(p => p.Id == planId, ct);
            if (plan is null)
                return Results.NotFound(new { message = "Plan no encontrado." });

            var name = request.Name.Trim();
            var nameTaken = await db.SubscriptionPlans.AsNoTracking().AnyAsync(p => p.Name == name && p.Id != planId, ct);
            if (nameTaken)
                return Results.Conflict(new { message = "Ya existe otro plan con ese nombre." });

            try
            {
                plan.UpdateDetails(name, request.Description, request.Price, request.BillingPeriodDays);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }

            if (request.IsActive) plan.Activate();
            else plan.Deactivate();

            await db.SaveChangesAsync(ct);

            return Results.Ok(new AdminSubscriptionPlanDto(plan.Id, plan.Name, plan.Description, plan.Price, plan.BillingPeriodDays, plan.IsActive));
        })
        .WithName("AdminUpdateSubscriptionPlan");

        // DELETE /admin/subscription-plans/{planId}
        group.MapDelete("/{planId:guid}", async (
            [FromRoute] Guid planId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var plan = await db.SubscriptionPlans.SingleOrDefaultAsync(p => p.Id == planId, ct);
            if (plan is null)
                return Results.NotFound(new { message = "Plan no encontrado." });

            db.SubscriptionPlans.Remove(plan);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("AdminDeleteSubscriptionPlan");

        // PATCH /admin/subscription-plans/{planId}/activate
        group.MapPatch("/{planId:guid}/activate", async (
            [FromRoute] Guid planId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var plan = await db.SubscriptionPlans.SingleOrDefaultAsync(p => p.Id == planId, ct);
            if (plan is null)
                return Results.NotFound(new { message = "Plan no encontrado." });

            plan.Activate();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { plan.Id, plan.IsActive });
        })
        .WithName("AdminActivateSubscriptionPlan");

        // PATCH /admin/subscription-plans/{planId}/deactivate
        group.MapPatch("/{planId:guid}/deactivate", async (
            [FromRoute] Guid planId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var plan = await db.SubscriptionPlans.SingleOrDefaultAsync(p => p.Id == planId, ct);
            if (plan is null)
                return Results.NotFound(new { message = "Plan no encontrado." });

            plan.Deactivate();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { plan.Id, plan.IsActive });
        })
        .WithName("AdminDeactivateSubscriptionPlan");

        return app;
    }
}
