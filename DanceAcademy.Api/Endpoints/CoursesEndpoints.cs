#nullable enable
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class CoursesEndpoints
{
    public static IEndpointRouteBuilder MapCoursesEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/")
            .WithTags("Courses")
            .RequireAuthorization(); // Requiere JWT para todo el grupo (MVP)

        // GET /courses
        group.MapGet("/courses", async (
            AppDbContext db,
            CancellationToken ct) =>
        {
            // Solo publicados (MVP)
            var courses = await db.Courses
                .AsNoTracking()
                .Where(c => c.IsPublished)
                .OrderBy(c => c.Title)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.Description
                })
                .ToListAsync(ct);

            return Results.Ok(courses);
        })
        .WithName("GetCourses");

        // GET /courses/{courseId}
        group.MapGet("/courses/{courseId:guid}", async (
            [FromRoute] Guid courseId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (courseId == Guid.Empty)
                return Results.BadRequest(new { message = "courseId inválido." });

            // Traemos el curso publicado + módulos publicados
            var course = await db.Courses
                .AsNoTracking()
                .Where(c => c.Id == courseId && c.IsPublished)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.Description,
                    Modules = c.Modules
                        .Where(m => m.IsPublished)
                        .OrderBy(m => m.Order)
                        .Select(m => new
                        {
                            m.Id,
                            m.Title,
                            m.Order
                        })
                        .ToList()
                })
                .SingleOrDefaultAsync(ct);

            return course is null
                ? Results.NotFound(new { message = "Curso no encontrado o no publicado." })
                : Results.Ok(course);
        })
        .WithName("GetCourseDetail");

        // GET /modules/{moduleId}/lessons
        group.MapGet("/modules/{moduleId:guid}/lessons", async (
            [FromRoute] Guid moduleId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (moduleId == Guid.Empty)
                return Results.BadRequest(new { message = "moduleId inválido." });

            var lessons = await db.Lessons
                .AsNoTracking()
                .Where(l => l.ModuleId == moduleId && l.IsPublished)
                .OrderBy(l => l.Order)
                .Select(l => new
                {
                    l.Id,
                    l.Title,
                    l.Order,
                    l.VideoUrl
                })
                .ToListAsync(ct);

            return Results.Ok(lessons);
        })
        .WithName("GetModuleLessons");

        return app;
    }
}
