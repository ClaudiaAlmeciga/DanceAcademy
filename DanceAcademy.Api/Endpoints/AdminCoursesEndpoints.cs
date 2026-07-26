#nullable enable
using DanceAcademy.Application.DTOs.Admin;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class AdminCoursesEndpoints
{
    public static IEndpointRouteBuilder MapAdminCoursesEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/admin")
            .WithTags("Admin - Courses")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        #region courses

        // GET /admin/courses
        group.MapGet("/courses", async (AppDbContext db, CancellationToken ct) =>
        {
            var courses = await db.Courses
                .AsNoTracking()
                .OrderBy(c => c.Title)
                .Select(c => new AdminCourseListItemDto(
                    c.Id,
                    c.Title,
                    c.Description,
                    c.Level.ToString(),
                    c.IsPublished,
                    c.Modules.Count,
                    c.Modules.SelectMany(m => m.Lessons).Count()
                ))
                .ToListAsync(ct);

            return Results.Ok(courses);
        })
        .WithName("AdminGetCourses");

        // GET /admin/courses/{courseId}
        group.MapGet("/courses/{courseId:guid}", async (
            [FromRoute] Guid courseId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (courseId == Guid.Empty)
                return Results.BadRequest(new { message = "courseId inválido." });

            var course = await db.Courses
                .AsNoTracking()
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
                .SingleOrDefaultAsync(c => c.Id == courseId, ct);

            if (course is null)
                return Results.NotFound(new { message = "Curso no encontrado." });

            var courseDetailDto = new AdminCourseDetailDto(
                course.Id,
                course.Title,
                course.Description,
                course.Level.ToString(),
                course.IsPublished,
                course.Modules
                    .OrderBy(m => m.Order)
                    .Select(m => new AdminModuleDto(
                        m.Id,
                        m.Title,
                        m.Order,
                        m.IsPublished,
                        m.Lessons
                            .OrderBy(l => l.Order)
                            .Select(l => new AdminLessonDto(
                                l.Id,
                                l.Title,
                                l.Order,
                                l.IsPublished,
                                l.VideoUrl,
                                l.Content
                            ))
                            .ToList()
                    ))
                    .ToList()
            );

            return Results.Ok(courseDetailDto);
        })
        .WithName("AdminGetCourseDetail");

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

            if (!Enum.TryParse<CourseLevel>(request.Level, ignoreCase: true, out var courseLevel))
                return Results.BadRequest(new { message = "Nivel inválido. Valores: Beginner, Intermediate, Advanced." });

            var title = request.Title.Trim();
            var description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

            var titleExists = await db.Courses
                .AsNoTracking()
                .AnyAsync(c => c.Title == title, ct);

            if (titleExists)
                return Results.Conflict(new { message = "Ya existe un curso con ese título." });

            var course = new Course(title, description, courseLevel, isPublished: false);

            db.Courses.Add(course);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/admin/courses/{course.Id}", new
            {
                course.Id,
                course.Title,
                course.Description,
                Level = course.Level.ToString(),
                course.IsPublished
            });
        })
        .WithName("AdminCreateCourse");

        // PUT /admin/courses/{courseId}
        group.MapPut("/courses/{courseId:guid}", async (
            [FromRoute] Guid courseId,
            [FromBody] UpdateCourseRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (courseId == Guid.Empty)
                return Results.BadRequest(new { message = "courseId inválido." });

            if (request is null || string.IsNullOrWhiteSpace(request.Title))
                return Results.BadRequest(new { message = "Title es obligatorio." });

            if (!Enum.TryParse<CourseLevel>(request.Level, ignoreCase: true, out var courseLevel))
                return Results.BadRequest(new { message = "Nivel inválido. Valores: Beginner, Intermediate, Advanced." });

            var course = await db.Courses.SingleOrDefaultAsync(c => c.Id == courseId, ct);
            if (course is null)
                return Results.NotFound(new { message = "Curso no encontrado." });

            if (course.IsPublished)
                return Results.Conflict(new { message = "No se puede editar un curso publicado." });

            var title = request.Title.Trim();
            var titleTaken = await db.Courses
                .AsNoTracking()
                .AnyAsync(c => c.Title == title && c.Id != courseId, ct);

            if (titleTaken)
                return Results.Conflict(new { message = "Ya existe otro curso con ese título." });

            course.UpdateDetails(title, string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim());
            course.SetLevel(courseLevel);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { course.Id, course.Title, course.Description, Level = course.Level.ToString(), course.IsPublished });
        })
        .WithName("AdminUpdateCourse");

        // DELETE /admin/courses/{courseId}
        group.MapDelete("/courses/{courseId:guid}", async (
            [FromRoute] Guid courseId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (courseId == Guid.Empty)
                return Results.BadRequest(new { message = "courseId inválido." });

            var course = await db.Courses.SingleOrDefaultAsync(c => c.Id == courseId, ct);
            if (course is null)
                return Results.NotFound(new { message = "Curso no encontrado." });

            db.Courses.Remove(course);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("AdminDeleteCourse");

        // PATCH /admin/courses/{courseId}/publish
        group.MapPatch("/courses/{courseId:guid}/publish", async (
            [FromRoute] Guid courseId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (courseId == Guid.Empty)
                return Results.BadRequest(new { message = "courseId inválido." });

            var course = await db.Courses.SingleOrDefaultAsync(c => c.Id == courseId, ct);

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

            var course = await db.Courses.SingleOrDefaultAsync(c => c.Id == courseId, ct);

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

            var courseExists = await db.Courses
                .AsNoTracking()
                .AnyAsync(c => c.Id == courseId, ct);

            if (!courseExists)
                return Results.NotFound(new { message = "Curso no encontrado." });

            var orderTaken = await db.Modules
                .AsNoTracking()
                .AnyAsync(m => m.CourseId == courseId && m.Order == request.Order, ct);

            if (orderTaken)
                return Results.Conflict(new { message = "Ya existe un módulo con ese orden para este curso." });

            var module = new Module(courseId: courseId, title: request.Title.Trim(), order: request.Order);

            db.Modules.Add(module);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/admin/courses/{courseId}", new
            {
                module.Id,
                module.CourseId,
                module.Title,
                module.Order,
                module.IsPublished
            });
        })
        .WithName("AdminCreateModule");

        // DELETE /admin/modules/{moduleId}
        group.MapDelete("/modules/{moduleId:guid}", async (
            Guid moduleId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (moduleId == Guid.Empty)
                return Results.BadRequest(new { message = "moduleId inválido." });

            var module = await db.Modules.SingleOrDefaultAsync(m => m.Id == moduleId, ct);
            if (module is null)
                return Results.NotFound(new { message = "Módulo no encontrado." });

            db.Modules.Remove(module);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("AdminDeleteModule");

        // PUT /admin/modules/{moduleId}
        group.MapPut("/modules/{moduleId:guid}", async (
            Guid moduleId,
            [FromBody] UpdateModuleRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (moduleId == Guid.Empty)
                return Results.BadRequest(new { message = "moduleId inválido." });

            if (request is null || string.IsNullOrWhiteSpace(request.Title))
                return Results.BadRequest(new { message = "Title es obligatorio." });

            if (request.Order < 1)
                return Results.BadRequest(new { message = "Order debe ser >= 1." });

            var module = await db.Modules.SingleOrDefaultAsync(m => m.Id == moduleId, ct);
            if (module is null)
                return Results.NotFound(new { message = "Módulo no encontrado." });

            if (module.IsPublished)
                return Results.Conflict(new { message = "No se puede editar un módulo publicado." });

            var orderTaken = await db.Modules
                .AsNoTracking()
                .AnyAsync(m => m.CourseId == module.CourseId && m.Order == request.Order && m.Id != moduleId, ct);

            if (orderTaken)
                return Results.Conflict(new { message = "Ya existe un módulo con ese orden en este curso." });

            module.UpdateDetails(request.Title.Trim(), request.Order);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { module.Id, module.Title, module.Order, module.IsPublished });
        })
        .WithName("AdminUpdateModule");

        // PATCH /admin/modules/{moduleId}/publish
        group.MapPatch("/modules/{moduleId:guid}/publish", async (
            Guid moduleId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (moduleId == Guid.Empty)
                return Results.BadRequest(new { message = "moduleId inválido." });

            var module = await db.Modules.SingleOrDefaultAsync(m => m.Id == moduleId, ct);

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

            var module = await db.Modules.SingleOrDefaultAsync(m => m.Id == moduleId, ct);

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

            var orderTaken = await db.Lessons
                .AsNoTracking()
                .AnyAsync(l => l.ModuleId == moduleId && l.Order == request.Order, ct);

            if (orderTaken)
                return Results.Conflict(new { message = "Ya existe una lección con ese orden para este módulo." });

            var lesson = Lesson.Create(
                moduleId: moduleId,
                title: request.Title.Trim(),
                order: request.Order,
                content: content,
                videoUrl: videoUrl
            );

            db.Lessons.Add(lesson);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/admin/modules/{moduleId}/lessons", new
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

        // DELETE /admin/lessons/{lessonId}
        group.MapDelete("/lessons/{lessonId:guid}", async (
            Guid lessonId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (lessonId == Guid.Empty)
                return Results.BadRequest(new { message = "lessonId inválido." });

            var lesson = await db.Lessons.SingleOrDefaultAsync(l => l.Id == lessonId, ct);
            if (lesson is null)
                return Results.NotFound(new { message = "Lección no encontrada." });

            db.Lessons.Remove(lesson);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("AdminDeleteLesson");

        // PUT /admin/lessons/{lessonId}
        group.MapPut("/lessons/{lessonId:guid}", async (
            Guid lessonId,
            [FromBody] UpdateLessonRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (lessonId == Guid.Empty)
                return Results.BadRequest(new { message = "lessonId inválido." });

            if (request is null || string.IsNullOrWhiteSpace(request.Title))
                return Results.BadRequest(new { message = "Title es obligatorio." });

            if (request.Order < 1)
                return Results.BadRequest(new { message = "Order debe ser >= 1." });

            var lesson = await db.Lessons.SingleOrDefaultAsync(l => l.Id == lessonId, ct);
            if (lesson is null)
                return Results.NotFound(new { message = "Lección no encontrada." });

            if (lesson.IsPublished)
                return Results.Conflict(new { message = "No se puede editar una lección publicada." });

            var orderTaken = await db.Lessons
                .AsNoTracking()
                .AnyAsync(l => l.ModuleId == lesson.ModuleId && l.Order == request.Order && l.Id != lessonId, ct);

            if (orderTaken)
                return Results.Conflict(new { message = "Ya existe una lección con ese orden en este módulo." });

            var videoUrl = string.IsNullOrWhiteSpace(request.VideoUrl) ? null : request.VideoUrl.Trim();
            var content = string.IsNullOrWhiteSpace(request.Content) ? null : request.Content.Trim();

            lesson.UpdateDetails(request.Title.Trim(), request.Order, content, videoUrl);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { lesson.Id, lesson.Title, lesson.Order, lesson.IsPublished });
        })
        .WithName("AdminUpdateLesson");

        // PATCH /admin/lessons/{lessonId}/publish
        group.MapPatch("/lessons/{lessonId:guid}/publish", async (
            Guid lessonId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (lessonId == Guid.Empty)
                return Results.BadRequest(new { message = "lessonId inválido." });

            var lesson = await db.Lessons.SingleOrDefaultAsync(l => l.Id == lessonId, ct);

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

            var lesson = await db.Lessons.SingleOrDefaultAsync(l => l.Id == lessonId, ct);

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
