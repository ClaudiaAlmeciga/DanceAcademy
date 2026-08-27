#nullable enable
using DanceAcademy.Application.DTOs.Public;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class PublicNewsEndpoints
{
    public static IEndpointRouteBuilder MapPublicNewsEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/public")
            .WithTags("Public - News");

        group.MapGet("/news", async (AppDbContext db, CancellationToken ct) =>
        {
            var news = await db.NewsPosts
                .AsNoTracking()
                .Where(n => n.IsPublished)
                .OrderByDescending(n => n.PublishedAt)
                .Select(n => new NewsPostDto(n.Id, n.Title, n.Content, n.ImageUrl, n.PublishedAt))
                .ToListAsync(ct);

            return Results.Ok(news);
        })
        .WithName("PublicGetNewsPosts");

        group.MapGet("/news/{newsPostId:guid}", async (
            [FromRoute] Guid newsPostId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var post = await db.NewsPosts.AsNoTracking().SingleOrDefaultAsync(n => n.Id == newsPostId && n.IsPublished, ct);
            if (post is null)
                return Results.NotFound(new { message = "Noticia no encontrada." });

            return Results.Ok(new NewsPostDto(post.Id, post.Title, post.Content, post.ImageUrl, post.PublishedAt));
        })
        .WithName("PublicGetNewsPostDetail");

        return app;
    }
}
