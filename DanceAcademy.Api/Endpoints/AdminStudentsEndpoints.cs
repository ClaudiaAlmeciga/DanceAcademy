#nullable enable
using DanceAcademy.Application.DTOs.Admin;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class AdminStudentsEndpoints
{
    public static IEndpointRouteBuilder MapAdminStudentsEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/admin/students")
            .WithTags("Admin - Students")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        // GET /admin/students — listado de estudiantes con resumen de inscripciones y avance
        group.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
        {
            var students = await db.Users
                .AsNoTracking()
                .Where(u => u.Role == Roles.Student)
                .OrderBy(u => u.Email)
                .ToListAsync(ct);

            var courseProgressByStudent = await GetCourseProgressByStudentAsync(db, studentId: null, ct);

            var studentListItems = students.Select(student =>
            {
                var courseProgressList = courseProgressByStudent.GetValueOrDefault(student.Id, []);
                var averageProgressPercentage = courseProgressList.Count == 0
                    ? 0d
                    : Math.Round(courseProgressList.Average(c => c.ProgressPercentage), 2);

                return new AdminStudentListItemDto(
                    student.Id,
                    student.Email,
                    student.FullName,
                    student.IsActive,
                    student.CreatedAt,
                    courseProgressList.Count,
                    averageProgressPercentage);
            }).ToList();

            return Results.Ok(studentListItems);
        })
        .WithName("AdminGetStudents");

        // GET /admin/students/{studentId} — detalle de un estudiante con avance curso por curso
        group.MapGet("/{studentId:guid}", async (
            [FromRoute] Guid studentId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var student = await db.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Id == studentId && u.Role == Roles.Student, ct);

            if (student is null)
                return Results.NotFound(new { message = "Estudiante no encontrado." });

            var courseProgressByStudent = await GetCourseProgressByStudentAsync(db, studentId, ct);
            var courseProgressList = courseProgressByStudent.GetValueOrDefault(studentId, []);

            var studentDetail = new AdminStudentDetailDto(
                student.Id,
                student.Email,
                student.FullName,
                student.Phone,
                student.IsActive,
                student.CreatedAt,
                courseProgressList
                    .OrderByDescending(c => c.EnrolledAt)
                    .ToList());

            return Results.Ok(studentDetail);
        })
        .WithName("AdminGetStudentDetail");

        return app;
    }

    /// <summary>
    /// Calcula el avance por curso de cada estudiante inscrito, a partir de Enrollment/LessonProgress
    /// existentes (sin entidades nuevas). Si <paramref name="studentId"/> se especifica, limita el
    /// cálculo a las inscripciones de ese estudiante (uso en detalle); si es null, calcula para todos
    /// los estudiantes inscritos (uso en listado).
    /// </summary>
    private static async Task<Dictionary<Guid, List<AdminStudentCourseProgressDto>>> GetCourseProgressByStudentAsync(
        AppDbContext db, Guid? studentId, CancellationToken ct)
    {
        var enrollmentsQuery = db.Enrollments.AsNoTracking();
        if (studentId is not null)
            enrollmentsQuery = enrollmentsQuery.Where(e => e.UserId == studentId.Value);

        var enrollments = await enrollmentsQuery
            .Select(e => new { e.UserId, e.CourseId, e.EnrolledAt })
            .ToListAsync(ct);

        if (enrollments.Count == 0)
            return [];

        var courseIds = enrollments.Select(e => e.CourseId).Distinct().ToList();

        var courses = await db.Courses
            .AsNoTracking()
            .Where(c => courseIds.Contains(c.Id))
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

        var courseLookup = courses.ToDictionary(c => c.Id);

        var completedLessonsByUserAndCourse = await (
            from lp in db.LessonProgresses.AsNoTracking()
            join l in db.Lessons.AsNoTracking() on lp.LessonId equals l.Id
            join m in db.Modules.AsNoTracking() on l.ModuleId equals m.Id
            where lp.IsCompleted && courseIds.Contains(m.CourseId)
            select new { lp.UserId, CourseId = m.CourseId })
            .ToListAsync(ct);

        var completedLessonCountLookup = completedLessonsByUserAndCourse
            .GroupBy(x => (x.UserId, x.CourseId))
            .ToDictionary(g => g.Key, g => g.Count());

        var courseProgressByStudent = new Dictionary<Guid, List<AdminStudentCourseProgressDto>>();

        foreach (var enrollment in enrollments)
        {
            if (!courseLookup.TryGetValue(enrollment.CourseId, out var course))
                continue; // curso eliminado — se omite del resumen de avance

            var completedLessons = completedLessonCountLookup.GetValueOrDefault((enrollment.UserId, enrollment.CourseId));
            var progressPercentage = course.TotalLessons == 0
                ? 0d
                : Math.Round(completedLessons * 100d / course.TotalLessons, 2);

            var courseProgress = new AdminStudentCourseProgressDto(
                course.Id,
                course.Title,
                enrollment.EnrolledAt,
                course.TotalLessons,
                completedLessons,
                progressPercentage);

            if (!courseProgressByStudent.TryGetValue(enrollment.UserId, out var courseProgressList))
            {
                courseProgressList = [];
                courseProgressByStudent[enrollment.UserId] = courseProgressList;
            }

            courseProgressList.Add(courseProgress);
        }

        return courseProgressByStudent;
    }
}
