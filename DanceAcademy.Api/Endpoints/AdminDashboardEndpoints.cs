#nullable enable
using DanceAcademy.Application.DTOs.Admin;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class AdminDashboardEndpoints
{
    public static IEndpointRouteBuilder MapAdminDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/admin/dashboard")
            .WithTags("Admin - Dashboard")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        // GET /admin/dashboard/summary
        group.MapGet("/summary", async (AppDbContext db, CancellationToken ct) =>
        {
            var totalStudents = await db.Users.AsNoTracking().CountAsync(u => u.Role == Roles.Student, ct);
            var totalPublishedCourses = await db.Courses.AsNoTracking().CountAsync(c => c.IsPublished, ct);
            var totalEnrollments = await db.Enrollments.AsNoTracking().CountAsync(ct);
            var totalActiveInstructors = await db.Instructors.AsNoTracking().CountAsync(i => i.IsActive, ct);
            var totalPublishedTestimonials = await db.Testimonials.AsNoTracking().CountAsync(t => t.IsPublished, ct);

            var courseStats = await GetCourseStatsAsync(db, ct);
            var averageCompletionRate = courseStats.Count == 0
                ? 0d
                : Math.Round(courseStats.Average(c => c.AverageCompletionRate), 2);

            var summaryDto = new AdminDashboardSummaryDto(
                totalStudents,
                totalPublishedCourses,
                totalEnrollments,
                averageCompletionRate,
                totalActiveInstructors,
                totalPublishedTestimonials);
            return Results.Ok(summaryDto);
        })
        .WithName("AdminGetDashboardSummary");

        // GET /admin/dashboard/courses — cursos ordenados por número de inscritos
        group.MapGet("/courses", async (AppDbContext db, CancellationToken ct) =>
        {
            var courseStats = await GetCourseStatsAsync(db, ct);
            return Results.Ok(courseStats.OrderByDescending(c => c.EnrollmentCount).ToList());
        })
        .WithName("AdminGetCourseStats");

        return app;
    }

    private static async Task<List<AdminCourseStatsDto>> GetCourseStatsAsync(AppDbContext db, CancellationToken ct)
    {
        var courses = await db.Courses
            .AsNoTracking()
            .Where(c => c.IsPublished)
            .Select(c => new
            {
                c.Id,
                c.Title,
                TotalLessons = c.Modules
                    .Where(m => m.IsPublished)
                    .SelectMany(m => m.Lessons)
                    .Count(l => l.IsPublished)
            })
            .ToListAsync(ct);

        var enrollments = await db.Enrollments
            .AsNoTracking()
            .Select(e => new { e.UserId, e.CourseId })
            .ToListAsync(ct);

        var completedCountByUserAndCourse = await (
            from lp in db.LessonProgresses.AsNoTracking()
            join l in db.Lessons.AsNoTracking() on lp.LessonId equals l.Id
            join m in db.Modules.AsNoTracking() on l.ModuleId equals m.Id
            where lp.IsCompleted
            select new { lp.UserId, CourseId = m.CourseId })
            .ToListAsync(ct);

        var completedCountLookup = completedCountByUserAndCourse
            .GroupBy(x => (x.UserId, x.CourseId))
            .ToDictionary(g => g.Key, g => g.Count());

        var courseStats = new List<AdminCourseStatsDto>();

        foreach (var course in courses)
        {
            var courseEnrollments = enrollments.Where(e => e.CourseId == course.Id).ToList();

            var completionRates = courseEnrollments.Select(e =>
            {
                var completedCount = completedCountLookup.GetValueOrDefault((e.UserId, e.CourseId));
                return course.TotalLessons == 0 ? 0d : completedCount * 100d / course.TotalLessons;
            }).ToList();

            var averageCompletionRate = completionRates.Count == 0
                ? 0d
                : Math.Round(completionRates.Average(), 2);

            courseStats.Add(new AdminCourseStatsDto(course.Id, course.Title, courseEnrollments.Count, averageCompletionRate));
        }

        return courseStats;
    }
}
