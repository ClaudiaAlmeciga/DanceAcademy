#nullable enable
using DanceAcademy.Application.DTOs;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DanceAcademy.Api.Endpoints;

/// <summary>
/// Autoenvío de testimonios por el estudiante autenticado — la única forma de crear un
/// testimonio en la plataforma (ver nota en <c>AdminTestimonialsEndpoints</c>).
/// </summary>
public static class MeTestimonialsEndpoints
{
    public static IEndpointRouteBuilder MapMeTestimonialsEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/me/testimonials")
            .WithTags("Me - Testimonials")
            .RequireAuthorization();

        // GET /me/testimonials — envíos propios del usuario autenticado, con su estado de moderación
        group.MapGet("/", async (
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var myTestimonials = await (
                from t in db.Testimonials.AsNoTracking()
                join c in db.Courses.AsNoTracking() on t.CourseId equals c.Id into courseJoin
                from c in courseJoin.DefaultIfEmpty()
                where t.UserId == userId
                orderby t.CreatedAt descending
                select new MyTestimonialDto(t.Id, t.Content, t.Rating, t.CourseId, c != null ? c.Title : null, t.IsPublished, t.CreatedAt))
                .ToListAsync(ct);

            return Results.Ok(myTestimonials);
        })
        .WithName("GetMyTestimonials");

        // POST /me/testimonials — el estudiante deja su propio comentario; queda pendiente de moderación
        group.MapPost("/", async (
            [FromBody] CreateTestimonialSelfRequest request,
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Content))
                return Results.BadRequest(new { message = "Content es obligatorio." });

            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
                return Results.NotFound(new { message = "Usuario no encontrado." });

            if (request.CourseId.HasValue)
            {
                // Solo se puede vincular un curso en el que el propio estudiante esté inscrito —
                // no cualquier curso del catálogo.
                var isEnrolledInCourse = await db.Enrollments
                    .AsNoTracking()
                    .AnyAsync(e => e.UserId == userId && e.CourseId == request.CourseId, ct);
                if (!isEnrolledInCourse)
                    return Results.BadRequest(new { message = "Solo puedes vincular un curso en el que estés inscrito." });
            }

            var studentName = string.IsNullOrWhiteSpace(user.FullName)
                ? user.Email.Split('@')[0]
                : user.FullName;

            Testimonial testimonial;
            try
            {
                testimonial = new Testimonial(userId, studentName, request.Content, request.Rating, request.CourseId, photoUrl: null);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }

            db.Testimonials.Add(testimonial);
            await db.SaveChangesAsync(ct);

            var courseTitle = request.CourseId is null
                ? null
                : await db.Courses.AsNoTracking().Where(c => c.Id == request.CourseId).Select(c => c.Title).SingleOrDefaultAsync(ct);

            var myTestimonialDto = new MyTestimonialDto(
                testimonial.Id, testimonial.Content, testimonial.Rating, testimonial.CourseId, courseTitle, testimonial.IsPublished, testimonial.CreatedAt);

            return Results.Created($"/me/testimonials/{testimonial.Id}", myTestimonialDto);
        })
        .WithName("CreateMyTestimonial");

        return app;
    }
}
