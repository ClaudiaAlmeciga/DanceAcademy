#nullable enable
using DanceAcademy.Application.DTOs;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DanceAcademy.Api.Endpoints;

public static class MeEnrollmentsEndpoints
{
    public static IEndpointRouteBuilder MapMeEnrollmentsEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/me/enrollments")
            .WithTags("Me - Enrollments")
            .RequireAuthorization();

        // GET /me/enrollments — cursos en los que el usuario autenticado está inscrito
        group.MapGet("/", async (
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var enrollments = await db.Enrollments
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrolledAt)
                .Join(
                    db.Courses.AsNoTracking(),
                    enrollment => enrollment.CourseId,
                    course => course.Id,
                    (enrollment, course) => new EnrollmentDto(enrollment.Id, course.Id, course.Title, enrollment.EnrolledAt))
                .ToListAsync(ct);

            return Results.Ok(enrollments);
        })
        .WithName("GetMyEnrollments");

        // POST /me/enrollments — inscribe al usuario autenticado en un curso publicado y gratuito
        group.MapPost("/", async (
            [FromBody] CreateEnrollmentRequest request,
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (request is null || request.CourseId == Guid.Empty)
                return Results.BadRequest(new { message = "CourseId es obligatorio." });

            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var course = await db.Courses.AsNoTracking().SingleOrDefaultAsync(c => c.Id == request.CourseId, ct);
            if (course is null || !course.IsPublished)
                return Results.NotFound(new { message = "Curso no encontrado." });

            // La inscripción a cursos de pago (compra individual o suscripción) se habilita
            // junto con la integración de Wompi (Fase 5). Mientras tanto, solo cursos gratuitos.
            if (course.PricingType != PricingType.Free)
                return Results.Conflict(new { message = "Este curso requiere compra o suscripción activa. La inscripción a cursos de pago estará disponible próximamente." });

            var isAlreadyEnrolled = await db.Enrollments
                .AsNoTracking()
                .AnyAsync(e => e.UserId == userId && e.CourseId == request.CourseId, ct);
            if (isAlreadyEnrolled)
                return Results.Conflict(new { message = "Ya estás inscrito en este curso." });

            var enrollment = new Enrollment(userId, request.CourseId);

            db.Enrollments.Add(enrollment);
            await db.SaveChangesAsync(ct);

            var enrollmentDto = new EnrollmentDto(enrollment.Id, course.Id, course.Title, enrollment.EnrolledAt);
            return Results.Created($"/me/enrollments/{enrollment.Id}", enrollmentDto);
        })
        .WithName("CreateMyEnrollment");

        return app;
    }
}
