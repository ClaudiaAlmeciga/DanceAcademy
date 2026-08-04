#nullable enable
using DanceAcademy.Application.DTOs.Admin;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class AdminInstructorsEndpoints
{
    public static IEndpointRouteBuilder MapAdminInstructorsEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/admin/instructors")
            .WithTags("Admin - Instructors")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        group.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
        {
            var instructors = await db.Instructors
                .AsNoTracking()
                .OrderBy(i => i.FullName)
                .Select(i => new AdminInstructorDto(i.Id, i.FullName, i.Specialty, i.Bio, i.PhotoUrl, i.IsActive))
                .ToListAsync(ct);

            return Results.Ok(instructors);
        })
        .WithName("AdminGetInstructors");

        group.MapGet("/{instructorId:guid}", async (
            [FromRoute] Guid instructorId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var instructor = await db.Instructors.AsNoTracking().SingleOrDefaultAsync(i => i.Id == instructorId, ct);
            if (instructor is null)
                return Results.NotFound(new { message = "Instructor no encontrado." });

            return Results.Ok(new AdminInstructorDto(instructor.Id, instructor.FullName, instructor.Specialty, instructor.Bio, instructor.PhotoUrl, instructor.IsActive));
        })
        .WithName("AdminGetInstructorDetail");

        group.MapPost("/", async (
            [FromBody] CreateInstructorRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Specialty))
                return Results.BadRequest(new { message = "FullName y Specialty son obligatorios." });

            var instructor = new Instructor(request.FullName, request.Specialty, request.Bio, request.PhotoUrl);

            db.Instructors.Add(instructor);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/admin/instructors/{instructor.Id}",
                new AdminInstructorDto(instructor.Id, instructor.FullName, instructor.Specialty, instructor.Bio, instructor.PhotoUrl, instructor.IsActive));
        })
        .WithName("AdminCreateInstructor");

        group.MapPut("/{instructorId:guid}", async (
            [FromRoute] Guid instructorId,
            [FromBody] UpdateInstructorRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Specialty))
                return Results.BadRequest(new { message = "FullName y Specialty son obligatorios." });

            var instructor = await db.Instructors.SingleOrDefaultAsync(i => i.Id == instructorId, ct);
            if (instructor is null)
                return Results.NotFound(new { message = "Instructor no encontrado." });

            instructor.UpdateDetails(request.FullName, request.Specialty, request.Bio, request.PhotoUrl);

            if (request.IsActive) instructor.Activate();
            else instructor.Deactivate();

            await db.SaveChangesAsync(ct);

            return Results.Ok(new AdminInstructorDto(instructor.Id, instructor.FullName, instructor.Specialty, instructor.Bio, instructor.PhotoUrl, instructor.IsActive));
        })
        .WithName("AdminUpdateInstructor");

        group.MapDelete("/{instructorId:guid}", async (
            [FromRoute] Guid instructorId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var instructor = await db.Instructors.SingleOrDefaultAsync(i => i.Id == instructorId, ct);
            if (instructor is null)
                return Results.NotFound(new { message = "Instructor no encontrado." });

            db.Instructors.Remove(instructor);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("AdminDeleteInstructor");

        return app;
    }
}
