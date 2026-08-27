#nullable enable
using DanceAcademy.Application.DTOs.Admin;
using DanceAcademy.Domain.Entities;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Api.Endpoints;

public static class AdminNewsEndpoints
{
    public static IEndpointRouteBuilder MapAdminNewsEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/admin/news")
            .WithTags("Admin - News")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        group.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
        {
            var news = await db.NewsPosts
                .AsNoTracking()
                .OrderByDescending(n => n.PublishedAt)
                .Select(n => new AdminNewsPostDto(n.Id, n.Title, n.Content, n.ImageUrl, n.PublishedAt, n.IsPublished))
                .ToListAsync(ct);

            return Results.Ok(news);
        })
        .WithName("AdminGetNewsPosts");

        group.MapGet("/{newsPostId:guid}", async (
            [FromRoute] Guid newsPostId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var post = await db.NewsPosts.AsNoTracking().SingleOrDefaultAsync(n => n.Id == newsPostId, ct);
            if (post is null)
                return Results.NotFound(new { message = "Noticia no encontrada." });

            return Results.Ok(new AdminNewsPostDto(post.Id, post.Title, post.Content, post.ImageUrl, post.PublishedAt, post.IsPublished));
        })
        .WithName("AdminGetNewsPostDetail");

        group.MapPost("/", async (
            [FromBody] CreateNewsPostRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
                return Results.BadRequest(new { message = "Title y Content son obligatorios." });

            var post = new NewsPost(request.Title, request.Content, request.ImageUrl, request.PublishedAt);

            db.NewsPosts.Add(post);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/admin/news/{post.Id}",
                new AdminNewsPostDto(post.Id, post.Title, post.Content, post.ImageUrl, post.PublishedAt, post.IsPublished));
        })
        .WithName("AdminCreateNewsPost");

        group.MapPut("/{newsPostId:guid}", async (
            [FromRoute] Guid newsPostId,
            [FromBody] UpdateNewsPostRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
                return Results.BadRequest(new { message = "Title y Content son obligatorios." });

            var post = await db.NewsPosts.SingleOrDefaultAsync(n => n.Id == newsPostId, ct);
            if (post is null)
                return Results.NotFound(new { message = "Noticia no encontrada." });

            post.UpdateDetails(request.Title, request.Content, request.ImageUrl, request.PublishedAt);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new AdminNewsPostDto(post.Id, post.Title, post.Content, post.ImageUrl, post.PublishedAt, post.IsPublished));
        })
        .WithName("AdminUpdateNewsPost");

        group.MapDelete("/{newsPostId:guid}", async (
            [FromRoute] Guid newsPostId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var post = await db.NewsPosts.SingleOrDefaultAsync(n => n.Id == newsPostId, ct);
            if (post is null)
                return Results.NotFound(new { message = "Noticia no encontrada." });

            db.NewsPosts.Remove(post);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("AdminDeleteNewsPost");

        group.MapPatch("/{newsPostId:guid}/publish", async (
            [FromRoute] Guid newsPostId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var post = await db.NewsPosts.SingleOrDefaultAsync(n => n.Id == newsPostId, ct);
            if (post is null)
                return Results.NotFound(new { message = "Noticia no encontrada." });

            post.Publish();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { post.Id, post.IsPublished });
        })
        .WithName("AdminPublishNewsPost");

        group.MapPatch("/{newsPostId:guid}/unpublish", async (
            [FromRoute] Guid newsPostId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var post = await db.NewsPosts.SingleOrDefaultAsync(n => n.Id == newsPostId, ct);
            if (post is null)
                return Results.NotFound(new { message = "Noticia no encontrada." });

            post.Unpublish();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { post.Id, post.IsPublished });
        })
        .WithName("AdminUnpublishNewsPost");

        return app;
    }
}
