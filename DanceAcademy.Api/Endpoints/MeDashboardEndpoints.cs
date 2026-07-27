#nullable enable
using DanceAcademy.Application.DTOs;
using DanceAcademy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DanceAcademy.Api.Endpoints;

public static class MeDashboardEndpoints
{
    public static IEndpointRouteBuilder MapMeDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        // GET /me/dashboard — resumen de cursos inscritos y avance del usuario autenticado
        app.MapGet("/me/dashboard", async (
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var enrolledCourses = await db.Enrollments
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .Join(db.Courses.AsNoTracking(), e => e.CourseId, c => c.Id, (e, c) => new { c.Id, c.Title })
                .ToListAsync(ct);

            var courseIds = enrolledCourses.Select(c => c.Id).ToList();

            var totalLessonsByCourse = await db.Modules
                .AsNoTracking()
                .Where(m => courseIds.Contains(m.CourseId) && m.IsPublished)
                .SelectMany(m => m.Lessons.Where(l => l.IsPublished).Select(_ => m.CourseId))
                .GroupBy(courseId => courseId)
                .Select(g => new { CourseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CourseId, x => x.Count, ct);

            var completedLessonsByCourse = await (
                from lp in db.LessonProgresses.AsNoTracking()
                join l in db.Lessons.AsNoTracking() on lp.LessonId equals l.Id
                join m in db.Modules.AsNoTracking() on l.ModuleId equals m.Id
                where lp.UserId == userId && lp.IsCompleted && courseIds.Contains(m.CourseId)
                select m.CourseId)
                .GroupBy(courseId => courseId)
                .Select(g => new { CourseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CourseId, x => x.Count, ct);

            var courseSummaries = enrolledCourses.Select(course =>
            {
                var totalLessons = totalLessonsByCourse.GetValueOrDefault(course.Id);
                var completedLessons = completedLessonsByCourse.GetValueOrDefault(course.Id);
                var progressPercentage = totalLessons == 0 ? 0d : Math.Round(completedLessons * 100d / totalLessons, 2);

                return new MyCourseProgressSummaryDto(course.Id, course.Title, totalLessons, completedLessons, progressPercentage);
            }).ToList();

            var overallProgressPercentage = courseSummaries.Count == 0
                ? 0d
                : Math.Round(courseSummaries.Average(c => c.ProgressPercentage), 2);

            var dashboardDto = new MyDashboardDto(courseSummaries.Count, overallProgressPercentage, courseSummaries);
            return Results.Ok(dashboardDto);
        })
        .WithName("GetMyDashboard")
        .WithTags("Me - Dashboard")
        .RequireAuthorization();

        return app;
    }
}
