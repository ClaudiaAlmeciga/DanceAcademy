#nullable enable
using DanceAcademy.Application.DTOs.Public;
using DanceAcademy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class PublicTestimonialsEndpoints
{
    public static IEndpointRouteBuilder MapPublicTestimonialsEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/public")
            .WithTags("Public - Testimonials");

        group.MapGet("/testimonials", async (AppDbContext db, CancellationToken ct) =>
        {
            var testimonials = await (
                from t in db.Testimonials.AsNoTracking()
                join c in db.Courses.AsNoTracking() on t.CourseId equals c.Id into courseJoin
                from c in courseJoin.DefaultIfEmpty()
                where t.IsPublished
                orderby t.CreatedAt descending
                select new TestimonialDto(t.Id, t.StudentName, t.Content, t.Rating, c != null ? c.Title : null, t.PhotoUrl))
                .ToListAsync(ct);

            return Results.Ok(testimonials);
        })
        .WithName("PublicGetTestimonials");

        return app;
    }
}
