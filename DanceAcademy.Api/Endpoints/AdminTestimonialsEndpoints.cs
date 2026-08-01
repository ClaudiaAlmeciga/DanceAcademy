#nullable enable
using DanceAcademy.Application.DTOs.Admin;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

/// <summary>
/// Moderación de testimonios. No expone creación ni edición: los testimonios solo se
/// originan del envío de un estudiante autenticado (ver <c>MeTestimonialsEndpoints</c>)
/// y quedan tal como los escribió — el Admin únicamente aprueba o rechaza su publicación.
/// </summary>
public static class AdminTestimonialsEndpoints
{
    public static IEndpointRouteBuilder MapAdminTestimonialsEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/admin/testimonials")
            .WithTags("Admin - Testimonials")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        group.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
        {
            var testimonials = await (
                from t in db.Testimonials.AsNoTracking()
                join c in db.Courses.AsNoTracking() on t.CourseId equals c.Id into courseJoin
                from c in courseJoin.DefaultIfEmpty()
                orderby t.IsPublished, t.CreatedAt descending
                select new AdminTestimonialDto(t.Id, t.StudentName, t.Content, t.Rating, t.CourseId, c != null ? c.Title : null, t.PhotoUrl, t.IsPublished))
                .ToListAsync(ct);

            return Results.Ok(testimonials);
        })
        .WithName("AdminGetTestimonials");

        group.MapGet("/{testimonialId:guid}", async (
            [FromRoute] Guid testimonialId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var testimonial = await db.Testimonials.AsNoTracking().SingleOrDefaultAsync(t => t.Id == testimonialId, ct);
            if (testimonial is null)
                return Results.NotFound(new { message = "Testimonio no encontrado." });

            var courseTitle = testimonial.CourseId is null
                ? null
                : await db.Courses.AsNoTracking().Where(c => c.Id == testimonial.CourseId).Select(c => c.Title).SingleOrDefaultAsync(ct);

            return Results.Ok(new AdminTestimonialDto(testimonial.Id, testimonial.StudentName, testimonial.Content, testimonial.Rating, testimonial.CourseId, courseTitle, testimonial.PhotoUrl, testimonial.IsPublished));
        })
        .WithName("AdminGetTestimonialDetail");

        // PATCH /{testimonialId}/publish — Aprobar: lo hace visible en /public/testimonials
        group.MapPatch("/{testimonialId:guid}/publish", async (
            [FromRoute] Guid testimonialId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var testimonial = await db.Testimonials.SingleOrDefaultAsync(t => t.Id == testimonialId, ct);
            if (testimonial is null)
                return Results.NotFound(new { message = "Testimonio no encontrado." });

            testimonial.Publish();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { testimonial.Id, testimonial.IsPublished });
        })
        .WithName("AdminPublishTestimonial");

        // PATCH /{testimonialId}/unpublish — Reprobar: lo oculta de /public/testimonials
        group.MapPatch("/{testimonialId:guid}/unpublish", async (
            [FromRoute] Guid testimonialId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var testimonial = await db.Testimonials.SingleOrDefaultAsync(t => t.Id == testimonialId, ct);
            if (testimonial is null)
                return Results.NotFound(new { message = "Testimonio no encontrado." });

            testimonial.Unpublish();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { testimonial.Id, testimonial.IsPublished });
        })
        .WithName("AdminUnpublishTestimonial");

        return app;
    }
}
