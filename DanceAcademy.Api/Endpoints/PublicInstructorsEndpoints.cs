#nullable enable
using DanceAcademy.Application.DTOs.Public;
using DanceAcademy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class PublicInstructorsEndpoints
{
    public static IEndpointRouteBuilder MapPublicInstructorsEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/public")
            .WithTags("Public - Instructors");

        group.MapGet("/instructors", async (AppDbContext db, CancellationToken ct) =>
        {
            var instructors = await db.Instructors
                .AsNoTracking()
                .Where(i => i.IsActive)
                .OrderBy(i => i.FullName)
                .Select(i => new InstructorDto(i.Id, i.FullName, i.Specialty, i.Bio, i.PhotoUrl))
                .ToListAsync(ct);

            return Results.Ok(instructors);
        })
        .WithName("PublicGetInstructors");

        return app;
    }
}
