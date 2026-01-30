#nullable enable
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class AdminCoursesEndpoints
{
    // DTO minimalista para el request (MVP)
    public sealed record CreateCourseRequest(string Title, string? Description);
    public sealed record CreateModuleRequest(string Title, int Order);

    public sealed record CreateLessonRequest(
        string Title,
        int Order,
        string? Content,
        string? VideoUrl
    );

    public static IEndpointRouteBuilder MapAdminCoursesEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/admin")
            .WithTags("Admin - Courses")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        #region courses
        // POST /admin/courses
        group.MapPost("/courses", async (
        [FromBody] CreateCourseRequest request,
        AppDbContext db,
        CancellationToken ct) =>
{
    if (request is null)
        return Results.BadRequest(new { message = "Body requerido." });

    if (string.IsNullOrWhiteSpace(request.Title))
        return Results.BadRequest(new { message = "Title es obligatorio." });

    var title = request.Title.Trim();
    var description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

    // Regla MVP: evitar duplicados por título (suave). Si no quieres, lo quitamos.
    var exists = await db.Courses
        .AsNoTracking()
        .AnyAsync(c => c.Title == title, ct);

    if (exists)
        return Results.Conflict(new { message = "Ya existe un curso con ese título." });

    var course = new Course(title, description, isPublished: false);

    db.Courses.Add(course);
    await db.SaveChangesAsync(ct);

    // Devuelve lo creado (sin exponer todo)
    return Results.Created($"/courses/{course.Id}", new
    {
        course.Id,
        course.Title,
        course.Description,
        course.IsPublished
    });
})
        .WithName("AdminCreateCourse");

        // PATCH /admin/courses/{courseId}/publish
        group.MapPatch("/courses/{courseId:guid}/publish", async (
        [FromRoute] Guid courseId,
        AppDbContext db,
        CancellationToken ct) =>
{
    if (courseId == Guid.Empty)
        return Results.BadRequest(new { message = "courseId inválido." });

    var course = await db.Courses
        .SingleOrDefaultAsync(c => c.Id == courseId, ct);

    if (course is null)
        return Results.NotFound(new { message = "Curso no encontrado." });

    course.Publish();
    await db.SaveChangesAsync(ct);

    return Results.Ok(new { course.Id, course.IsPublished });
})
        .WithName("AdminPublishCourse");

        // PATCH /admin/courses/{courseId}/unpublish
        group.MapPatch("/courses/{courseId:guid}/unpublish", async (
    [FromRoute] Guid courseId,
    AppDbContext db,
    CancellationToken ct) =>
{
    if (courseId == Guid.Empty)
        return Results.BadRequest(new { message = "courseId inválido." });

    var course = await db.Courses
        .SingleOrDefaultAsync(c => c.Id == courseId, ct);

    if (course is null)
        return Results.NotFound(new { message = "Curso no encontrado." });

    course.Unpublish();
    await db.SaveChangesAsync(ct);

    return Results.Ok(new { course.Id, course.IsPublished });
})
        .WithName("AdminUnpublishCourse");
        #endregion

        #region modules
        // POST /admin/courses/{courseId}/modules
        group.MapPost("/courses/{courseId:guid}/modules", async (
    [FromRoute] Guid courseId,
    [FromBody] CreateModuleRequest request,
    AppDbContext db,
    CancellationToken ct) =>
{
    if (courseId == Guid.Empty)
        return Results.BadRequest(new { message = "courseId inválido." });

    if (request is null)
        return Results.BadRequest(new { message = "Body requerido." });

    if (string.IsNullOrWhiteSpace(request.Title))
        return Results.BadRequest(new { message = "Title es obligatorio." });

    if (request.Order < 1)
        return Results.BadRequest(new { message = "Order debe ser >= 1." });

    // Validar que el curso exista
    var courseExists = await db.Courses
        .AsNoTracking()
        .AnyAsync(c => c.Id == courseId, ct);

    if (!courseExists)
        return Results.NotFound(new { message = "Curso no encontrado." });

    // Validar orden único (mejor error que una excepción SQL)
    var orderTaken = await db.Modules
        .AsNoTracking()
        .AnyAsync(m => m.CourseId == courseId && m.Order == request.Order, ct);

    if (orderTaken)
        return Results.Conflict(new { message = "Ya existe un módulo con ese orden para este curso." });

    var module = new Module(courseId: courseId, title: request.Title.Trim(), order: request.Order);

    db.Modules.Add(module);
    await db.SaveChangesAsync(ct);

    return Results.Created($"/courses/{courseId}", new
    {
        module.Id,
        module.CourseId,
        module.Title,
        module.Order,
        module.IsPublished
    });
})
        .WithName("AdminCreateModule");

        // PATCH /admin/modules/{moduleId}/publish
        group.MapPatch("/modules/{moduleId:guid}/publish", async (
    Guid moduleId,
    AppDbContext db,
    CancellationToken ct) =>
{
    if (moduleId == Guid.Empty)
        return Results.BadRequest(new { message = "moduleId inválido." });

    var module = await db.Modules
        .SingleOrDefaultAsync(m => m.Id == moduleId, ct);

    if (module is null)
        return Results.NotFound(new { message = "Módulo no encontrado." });

    module.Publish();
    await db.SaveChangesAsync(ct);

    return Results.Ok(new { module.Id, module.IsPublished });
})
        .WithName("AdminPublishModule");

        // PATCH /admin/modules/{moduleId}/unpublish
        group.MapPatch("/modules/{moduleId:guid}/unpublish", async (
    Guid moduleId,
    AppDbContext db,
    CancellationToken ct) =>
{
    if (moduleId == Guid.Empty)
        return Results.BadRequest(new { message = "moduleId inválido." });

    var module = await db.Modules
        .SingleOrDefaultAsync(m => m.Id == moduleId, ct);

    if (module is null)
        return Results.NotFound(new { message = "Módulo no encontrado." });

    module.Unpublish();
    await db.SaveChangesAsync(ct);

    return Results.Ok(new { module.Id, module.IsPublished });
})
        .WithName("AdminUnpublishModule");
        #endregion

        #region lessons
        // POST /admin/modules/{moduleId}/lessons
        group.MapPost("/modules/{moduleId:guid}/lessons", async (
            Guid moduleId,
            CreateLessonRequest request,
            AppDbContext db,
            CancellationToken ct) =>
    {
        if (moduleId == Guid.Empty)
            return Results.BadRequest(new { message = "moduleId inválido." });

        if (request is null)
            return Results.BadRequest(new { message = "Body requerido." });

        if (string.IsNullOrWhiteSpace(request.Title))
            return Results.BadRequest(new { message = "Title es obligatorio." });

        if (request.Order < 1)
            return Results.BadRequest(new { message = "Order debe ser >= 1." });

        var videoUrl = string.IsNullOrWhiteSpace(request.VideoUrl) ? null : request.VideoUrl.Trim();
        if (videoUrl is not null && videoUrl.Length > 2000)
            return Results.BadRequest(new { message = "VideoUrl es demasiado largo." });

        var content = string.IsNullOrWhiteSpace(request.Content) ? null : request.Content.Trim();
        if (content is not null && content.Length > 8000)
            return Results.BadRequest(new { message = "Content es demasiado largo." });

        var moduleExists = await db.Modules
.AsNoTracking()
.AnyAsync(m => m.Id == moduleId, ct);

        if (!moduleExists)
            return Results.NotFound(new { message = "Módulo no encontrado." });

        // Validar orden único
        var orderTaken = await db.Lessons
            .AsNoTracking()
            .AnyAsync(l => l.ModuleId == moduleId && l.Order == request.Order, ct);

        if (orderTaken)
            return Results.Conflict(new { message = "Ya existe una lección con ese orden para este módulo." });

        // Crear lección sin tocar Module (evita concurrencia)
        var lesson = Lesson.Create(
            moduleId: moduleId,
            title: request.Title.Trim(),
            order: request.Order,
            content: content,
            videoUrl: videoUrl
        );

        db.Lessons.Add(lesson);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/modules/{moduleId}/lessons", new
        {
            lesson.Id,
            lesson.ModuleId,
            lesson.Title,
            lesson.Order,
            lesson.IsPublished,
            lesson.VideoUrl
        });
    })
        .WithName("AdminCreateLesson");

        // PATCH /admin/lessons/{lessonId}/publish
        group.MapPatch("/lessons/{lessonId:guid}/publish", async (
        Guid lessonId,
        AppDbContext db,
        CancellationToken ct) =>
    {
        if (lessonId == Guid.Empty)
            return Results.BadRequest(new { message = "lessonId inválido." });

        var lesson = await db.Lessons
            .SingleOrDefaultAsync(l => l.Id == lessonId, ct);

        if (lesson is null)
            return Results.NotFound(new { message = "Lección no encontrada." });

        lesson.Publish();
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { lesson.Id, lesson.IsPublished });
    })
        .WithName("AdminPublishLesson");

        // PATCH /admin/lessons/{lessonId}/unpublish
        group.MapPatch("/lessons/{lessonId:guid}/unpublish", async (
        Guid lessonId,
        AppDbContext db,
        CancellationToken ct) =>
    {
        if (lessonId == Guid.Empty)
            return Results.BadRequest(new { message = "lessonId inválido." });

        var lesson = await db.Lessons
            .SingleOrDefaultAsync(l => l.Id == lessonId, ct);

        if (lesson is null)
            return Results.NotFound(new { message = "Lección no encontrada." });

        lesson.Unpublish();
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { lesson.Id, lesson.IsPublished });
    })
        .WithName("AdminUnpublishLesson");
        #endregion

        return app;
    }
}