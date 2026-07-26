#nullable enable
using DanceAcademy.Application.DTOs.Public;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class PublicCoursesEndpoints
{
    public static IEndpointRouteBuilder MapPublicCoursesEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/public")
            .WithTags("Public - Courses"); // Sin RequireAuthorization

        //catálogo paginado + filtro level
        group.MapGet("/courses", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? level,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var query = new CoursesQuery(
                Page: page == 0 ? 1 : page,
                PageSize: pageSize == 0 ? 12 : pageSize,
                Level: string.IsNullOrWhiteSpace(level) ? null : level.Trim()
            );

            var (isValid, error) = query.Validate();
            if (!isValid) return Results.BadRequest(new { message = error });

            var coursesQ = db.Courses.AsNoTracking().Where(c => c.IsPublished);

            if (query.Level is not null)
            {
                // Filtro por enum guardado como string
                coursesQ = coursesQ.Where(c => c.Level.ToString().ToLowerInvariant() == query.Level.ToLowerInvariant());
            }

            var total = await coursesQ.CountAsync(ct);

            var items = await coursesQ
                .OrderBy(c => c.Title)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new CourseListItemDto(
                    c.Id,
                    c.Title,
                    c.Description,
                    c.Level.ToString()
                ))
                .ToListAsync(ct);

            return Results.Ok(new PagedResult<CourseListItemDto>(items, query.Page, query.PageSize, total));
        })
        .WithName("PublicGetCoursesPaged");

        //detalle curso
        group.MapGet("/courses/{courseId:guid}", async (
            [FromRoute] Guid courseId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (courseId == Guid.Empty)
                return Results.BadRequest(new { message = "courseId inválido." });

            var course = await db.Courses
                .AsNoTracking()
                .Where(c => c.Id == courseId && c.IsPublished)
                .Select(c => new CourseDetailDto(
                    c.Id,
                    c.Title,
                    c.Description,
                    c.Level.ToString(),
                    c.Modules
                        .Where(m => m.IsPublished)
                        .OrderBy(m => m.Order)
                        .Select(m => new ModuleDto(m.Id, m.Title, m.Order))
                        .ToList()
                ))
                .SingleOrDefaultAsync(ct);

            return course is null
                ? Results.NotFound(new { message = "Curso no encontrado o no publicado." })
                : Results.Ok(course);
        })
        .WithName("PublicGetCourseDetail");

        return app;
    }
}