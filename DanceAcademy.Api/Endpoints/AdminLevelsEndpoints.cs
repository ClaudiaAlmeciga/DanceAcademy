#nullable enable
using DanceAcademy.Application.DTOs.Admin;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class AdminLevelsEndpoints
{
    public static IEndpointRouteBuilder MapAdminLevelsEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/admin/levels")
            .WithTags("Admin - Levels")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        // GET /admin/levels
        group.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
        {
            var levels = await db.Levels
                .AsNoTracking()
                .OrderBy(l => l.Order)
                .Select(l => new AdminLevelDto(
                    l.Id,
                    l.Name,
                    l.Order,
                    l.IsActive,
                    db.Courses.Count(c => c.LevelId == l.Id)
                ))
                .ToListAsync(ct);

            return Results.Ok(levels);
        })
        .WithName("AdminGetLevels");

        // GET /admin/levels/{levelId}
        group.MapGet("/{levelId:guid}", async (
            [FromRoute] Guid levelId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var level = await db.Levels.AsNoTracking().SingleOrDefaultAsync(l => l.Id == levelId, ct);
            if (level is null)
                return Results.NotFound(new { message = "Nivel no encontrado." });

            var courseCount = await db.Courses.CountAsync(c => c.LevelId == levelId, ct);

            return Results.Ok(new AdminLevelDto(level.Id, level.Name, level.Order, level.IsActive, courseCount));
        })
        .WithName("AdminGetLevelDetail");

        // POST /admin/levels
        group.MapPost("/", async (
            [FromBody] CreateLevelRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { message = "Name es obligatorio." });

            if (request.Order < 1)
                return Results.BadRequest(new { message = "Order debe ser >= 1." });

            var name = request.Name.Trim();
            var nameExists = await db.Levels.AsNoTracking().AnyAsync(l => l.Name == name, ct);
            if (nameExists)
                return Results.Conflict(new { message = "Ya existe un nivel con ese nombre." });

            var level = new Level(name, request.Order);

            db.Levels.Add(level);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/admin/levels/{level.Id}", new AdminLevelDto(level.Id, level.Name, level.Order, level.IsActive, 0));
        })
        .WithName("AdminCreateLevel");

        // PUT /admin/levels/{levelId}
        group.MapPut("/{levelId:guid}", async (
            [FromRoute] Guid levelId,
            [FromBody] UpdateLevelRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { message = "Name es obligatorio." });

            if (request.Order < 1)
                return Results.BadRequest(new { message = "Order debe ser >= 1." });

            var level = await db.Levels.SingleOrDefaultAsync(l => l.Id == levelId, ct);
            if (level is null)
                return Results.NotFound(new { message = "Nivel no encontrado." });

            var name = request.Name.Trim();
            var nameTaken = await db.Levels.AsNoTracking().AnyAsync(l => l.Name == name && l.Id != levelId, ct);
            if (nameTaken)
                return Results.Conflict(new { message = "Ya existe otro nivel con ese nombre." });

            level.UpdateDetails(name, request.Order);

            if (request.IsActive) level.Activate();
            else level.Deactivate();

            await db.SaveChangesAsync(ct);

            var courseCount = await db.Courses.CountAsync(c => c.LevelId == levelId, ct);
            return Results.Ok(new AdminLevelDto(level.Id, level.Name, level.Order, level.IsActive, courseCount));
        })
        .WithName("AdminUpdateLevel");

        // DELETE /admin/levels/{levelId}
        group.MapDelete("/{levelId:guid}", async (
            [FromRoute] Guid levelId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var level = await db.Levels.SingleOrDefaultAsync(l => l.Id == levelId, ct);
            if (level is null)
                return Results.NotFound(new { message = "Nivel no encontrado." });

            var hasCourses = await db.Courses.AnyAsync(c => c.LevelId == levelId, ct);
            if (hasCourses)
                return Results.Conflict(new { message = "No se puede eliminar un nivel que tiene cursos asociados." });

            db.Levels.Remove(level);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("AdminDeleteLevel");

        // PATCH /admin/levels/{levelId}/activate
        group.MapPatch("/{levelId:guid}/activate", async (
            [FromRoute] Guid levelId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var level = await db.Levels.SingleOrDefaultAsync(l => l.Id == levelId, ct);
            if (level is null)
                return Results.NotFound(new { message = "Nivel no encontrado." });

            level.Activate();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { level.Id, level.IsActive });
        })
        .WithName("AdminActivateLevel");

        // PATCH /admin/levels/{levelId}/deactivate
        group.MapPatch("/{levelId:guid}/deactivate", async (
            [FromRoute] Guid levelId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var level = await db.Levels.SingleOrDefaultAsync(l => l.Id == levelId, ct);
            if (level is null)
                return Results.NotFound(new { message = "Nivel no encontrado." });

            level.Deactivate();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { level.Id, level.IsActive });
        })
        .WithName("AdminDeactivateLevel");

        return app;
    }
}
