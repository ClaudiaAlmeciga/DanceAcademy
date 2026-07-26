#nullable enable
using DanceAcademy.Application.DTOs.Public;
using DanceAcademy.Application.Helpers;
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

        //catálogo paginado + filtro levelId
        group.MapGet("/courses", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] Guid? levelId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var query = new CoursesQuery(
                Page: page == 0 ? 1 : page,
                PageSize: pageSize == 0 ? 12 : pageSize,
                LevelId: levelId
            );

            var (isValid, error) = query.Validate();
            if (!isValid) return Results.BadRequest(new { message = error });

            var coursesQ = db.Courses.AsNoTracking().Where(c => c.IsPublished);

            if (query.LevelId is not null)
                coursesQ = coursesQ.Where(c => c.LevelId == query.LevelId);

            var total = await coursesQ.CountAsync(ct);

            var items = await coursesQ
                .OrderBy(c => c.Title)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new CourseListItemDto(
                    c.Id,
                    c.Title,
                    c.Description,
                    c.LevelId,
                    db.Levels.Where(l => l.Id == c.LevelId).Select(l => l.Name).SingleOrDefault() ?? "",
                    c.PricingType,
                    c.Price
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
                    c.LevelId,
                    db.Levels.Where(l => l.Id == c.LevelId).Select(l => l.Name).SingleOrDefault() ?? "",
                    c.PricingType,
                    c.Price,
                    c.SubscriptionPlans
                        .Where(p => p.IsActive)
                        .Select(p => new SubscriptionPlanDto(p.Id, p.Name, p.Description, p.Price, p.BillingPeriodDays))
                        .ToList(),
                    c.Modules
                        .Where(m => m.IsPublished)
                        .OrderBy(m => m.Order)
                        .Select(m => new ModuleDto(
                            m.Id,
                            m.Title,
                            m.Order,
                            m.Lessons
                                .Where(l => l.IsPublished)
                                .OrderBy(l => l.Order)
                                .Select(l => new LessonDto(l.Id, l.Title, l.Order, l.VideoUrl))
                                .ToList()
                        ))
                        .ToList()
                ))
                .SingleOrDefaultAsync(ct);

            return course is null
                ? Results.NotFound(new { message = "Curso no encontrado o no publicado." })
                : Results.Ok(course);
        })
        .WithName("PublicGetCourseDetail");

        // detalle de lección — solo si lección, módulo y curso están publicados
        group.MapGet("/lessons/{lessonId:guid}", async (
            [FromRoute] Guid lessonId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (lessonId == Guid.Empty)
                return Results.BadRequest(new { message = "lessonId inválido." });

            var lesson = await (
                from l in db.Lessons.AsNoTracking()
                join m in db.Modules.AsNoTracking() on l.ModuleId equals m.Id
                join c in db.Courses.AsNoTracking() on m.CourseId equals c.Id
                where l.Id == lessonId && l.IsPublished && m.IsPublished && c.IsPublished
                select new
                {
                    l.Id,
                    l.ModuleId,
                    CourseId = c.Id,
                    l.Title,
                    l.Content,
                    l.VideoUrl
                })
                .SingleOrDefaultAsync(ct);

            if (lesson is null)
                return Results.NotFound(new { message = "Lección no encontrada o no publicada." });

            var embed = VideoEmbedHelper.Resolve(lesson.VideoUrl);

            var dto = new LessonDetailDto(
                lesson.Id,
                lesson.ModuleId,
                lesson.CourseId,
                lesson.Title,
                lesson.Content,
                lesson.VideoUrl,
                embed?.EmbedUrl,
                embed?.IsDirect ?? false
            );

            return Results.Ok(dto);
        })
        .WithName("PublicGetLessonDetail");

        return app;
    }
}