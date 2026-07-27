#nullable enable
using DanceAcademy.Application.DTOs;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DanceAcademy.Api.Endpoints;

public static class MeProgressEndpoints
{
    public static IEndpointRouteBuilder MapMeProgressEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/me/progress")
            .WithTags("Me - Progress")
            .RequireAuthorization();

        // GET /me/progress/lessons/{lessonId} — estado de avance del usuario en una lección puntual
        group.MapGet("/lessons/{lessonId:guid}", async (
            [FromRoute] Guid lessonId,
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (lessonId == Guid.Empty)
                return Results.BadRequest(new { message = "lessonId inválido." });

            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var lessonInfo = await (
                from l in db.Lessons.AsNoTracking()
                join m in db.Modules.AsNoTracking() on l.ModuleId equals m.Id
                join c in db.Courses.AsNoTracking() on m.CourseId equals c.Id
                where l.Id == lessonId
                select new { CourseId = c.Id })
                .SingleOrDefaultAsync(ct);

            if (lessonInfo is null)
                return Results.NotFound(new { message = "Lección no encontrada." });

            var isEnrolledInCourse = await db.Enrollments
                .AsNoTracking()
                .AnyAsync(e => e.UserId == userId && e.CourseId == lessonInfo.CourseId, ct);

            if (!isEnrolledInCourse)
                return Results.Forbid();

            var lessonProgress = await db.LessonProgresses
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId, ct);

            var lessonProgressDto = new LessonProgressDto(lessonId, lessonProgress?.IsCompleted ?? false, lessonProgress?.CompletedAt);
            return Results.Ok(lessonProgressDto);
        })
        .WithName("GetMyLessonProgress");

        // POST /me/progress/lessons/{lessonId}/complete — marca la lección como completada
        group.MapPost("/lessons/{lessonId:guid}/complete", async (
            [FromRoute] Guid lessonId,
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (lessonId == Guid.Empty)
                return Results.BadRequest(new { message = "lessonId inválido." });

            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var lessonInfo = await (
                from l in db.Lessons.AsNoTracking()
                join m in db.Modules.AsNoTracking() on l.ModuleId equals m.Id
                join c in db.Courses.AsNoTracking() on m.CourseId equals c.Id
                where l.Id == lessonId
                select new
                {
                    LessonIsPublished = l.IsPublished,
                    ModuleIsPublished = m.IsPublished,
                    CourseId = c.Id,
                    CourseIsPublished = c.IsPublished
                })
                .SingleOrDefaultAsync(ct);

            // Early return: el análisis de flujo de nullable garantiza que, tras este chequeo,
            // lessonInfo no es null en el resto del handler — no se requiere '!'.
            if (lessonInfo is null || !lessonInfo.LessonIsPublished || !lessonInfo.ModuleIsPublished || !lessonInfo.CourseIsPublished)
                return Results.NotFound(new { message = "Lección no encontrada o no publicada." });

            var isEnrolledInCourse = await db.Enrollments
                .AsNoTracking()
                .AnyAsync(e => e.UserId == userId && e.CourseId == lessonInfo.CourseId, ct);

            if (!isEnrolledInCourse)
                return Results.Forbid();

            var lessonProgress = await db.LessonProgresses
                .SingleOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId, ct);

            if (lessonProgress is null)
            {
                lessonProgress = new LessonProgress(userId, lessonId);
                db.LessonProgresses.Add(lessonProgress);
            }

            lessonProgress.MarkCompleted();

            await db.SaveChangesAsync(ct);

            var completedProgressDto = new LessonProgressDto(lessonId, lessonProgress.IsCompleted, lessonProgress.CompletedAt);
            return Results.Ok(completedProgressDto);
        })
        .WithName("MarkLessonCompleted");

        // DELETE /me/progress/lessons/{lessonId}/complete — revierte una lección marcada como completada
        group.MapDelete("/lessons/{lessonId:guid}/complete", async (
            [FromRoute] Guid lessonId,
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (lessonId == Guid.Empty)
                return Results.BadRequest(new { message = "lessonId inválido." });

            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var lessonProgress = await db.LessonProgresses
                .SingleOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId, ct);

            if (lessonProgress is null)
                return Results.NotFound(new { message = "No hay avance registrado para esta lección." });

            lessonProgress.MarkIncomplete();

            await db.SaveChangesAsync(ct);

            var revertedProgressDto = new LessonProgressDto(lessonId, lessonProgress.IsCompleted, lessonProgress.CompletedAt);
            return Results.Ok(revertedProgressDto);
        })
        .WithName("UnmarkLessonCompleted");

        // GET /me/progress/courses/{courseId} — % de avance del usuario autenticado en el curso
        group.MapGet("/courses/{courseId:guid}", async (
            [FromRoute] Guid courseId,
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (courseId == Guid.Empty)
                return Results.BadRequest(new { message = "courseId inválido." });

            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var course = await db.Courses
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == courseId && c.IsPublished, ct);

            if (course is null)
                return Results.NotFound(new { message = "Curso no encontrado o no publicado." });

            var isEnrolledInCourse = await db.Enrollments
                .AsNoTracking()
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId, ct);

            if (!isEnrolledInCourse)
                return Results.Forbid();

            var publishedLessonIds = await (
                from l in db.Lessons.AsNoTracking()
                join m in db.Modules.AsNoTracking() on l.ModuleId equals m.Id
                where m.CourseId == courseId && l.IsPublished && m.IsPublished
                select l.Id)
                .ToListAsync(ct);

            var totalLessons = publishedLessonIds.Count;

            var completedLessons = totalLessons == 0
                ? 0
                : await db.LessonProgresses
                    .AsNoTracking()
                    .CountAsync(p => p.UserId == userId && p.IsCompleted && publishedLessonIds.Contains(p.LessonId), ct);

            var progressPercentage = totalLessons == 0
                ? 0d
                : Math.Round(completedLessons * 100d / totalLessons, 2);

            var courseProgressDto = new CourseProgressDto(course.Id, course.Title, totalLessons, completedLessons, progressPercentage);
            return Results.Ok(courseProgressDto);
        })
        .WithName("GetMyCourseProgress");

        // GET /me/progress/courses/{courseId}/lessons — lecciones del curso (ordenadas) con su estado de avance
        group.MapGet("/courses/{courseId:guid}/lessons", async (
            [FromRoute] Guid courseId,
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (courseId == Guid.Empty)
                return Results.BadRequest(new { message = "courseId inválido." });

            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var courseExists = await db.Courses
                .AsNoTracking()
                .AnyAsync(c => c.Id == courseId && c.IsPublished, ct);

            if (!courseExists)
                return Results.NotFound(new { message = "Curso no encontrado o no publicado." });

            var isEnrolledInCourse = await db.Enrollments
                .AsNoTracking()
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId, ct);

            if (!isEnrolledInCourse)
                return Results.Forbid();

            var lessons = await (
                from l in db.Lessons.AsNoTracking()
                join m in db.Modules.AsNoTracking() on l.ModuleId equals m.Id
                where m.CourseId == courseId && l.IsPublished && m.IsPublished
                orderby m.Order, l.Order
                select new { l.Id, l.ModuleId, ModuleTitle = m.Title, LessonTitle = l.Title, l.Order })
                .ToListAsync(ct);

            var completedLessonIds = await db.LessonProgresses
                .AsNoTracking()
                .Where(p => p.UserId == userId && p.IsCompleted)
                .Select(p => p.LessonId)
                .ToListAsync(ct);
            var completedLessonIdSet = completedLessonIds.ToHashSet();

            var lessonProgressList = lessons
                .Select(l => new LessonProgressListItemDto(
                    l.Id, l.ModuleId, l.ModuleTitle, l.LessonTitle, l.Order, completedLessonIdSet.Contains(l.Id)))
                .ToList();

            return Results.Ok(lessonProgressList);
        })
        .WithName("GetMyCourseLessonProgress");

        return app;
    }
}
