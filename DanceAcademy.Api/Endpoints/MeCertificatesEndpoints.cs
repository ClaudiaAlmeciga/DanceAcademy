#nullable enable
using DanceAcademy.Application.DTOs;
using DanceAcademy.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DanceAcademy.Api.Endpoints;

public static class MeCertificatesEndpoints
{
    public static IEndpointRouteBuilder MapMeCertificatesEndpoints(this IEndpointRouteBuilder app)
    {
        if (app is null) throw new ArgumentNullException(nameof(app));

        var group = app.MapGroup("/me/certificates")
            .WithTags("Me - Certificates")
            .RequireAuthorization();

        // GET /me/certificates — todos los certificados del usuario autenticado
        group.MapGet("/", async (
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var certificates = await (
                from cert in db.Certificates.AsNoTracking()
                join c in db.Courses.AsNoTracking() on cert.CourseId equals c.Id
                where cert.UserId == userId
                orderby cert.IssuedAt descending
                select new CertificateDto(cert.Id, cert.CourseId, c.Title, cert.VerificationCode, cert.IssuedAt))
                .ToListAsync(ct);

            return Results.Ok(certificates);
        })
        .WithName("GetMyCertificates");

        // GET /me/certificates/{courseId} — certificado del usuario para un curso puntual (404 si no existe aún)
        group.MapGet("/{courseId:guid}", async (
            [FromRoute] Guid courseId,
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var certificate = await (
                from cert in db.Certificates.AsNoTracking()
                join c in db.Courses.AsNoTracking() on cert.CourseId equals c.Id
                where cert.UserId == userId && cert.CourseId == courseId
                select new CertificateDto(cert.Id, cert.CourseId, c.Title, cert.VerificationCode, cert.IssuedAt))
                .SingleOrDefaultAsync(ct);

            if (certificate is null)
                return Results.NotFound(new { message = "Aún no se ha emitido un certificado para este curso." });

            return Results.Ok(certificate);
        })
        .WithName("GetMyCertificateForCourse");

        return app;
    }
}
