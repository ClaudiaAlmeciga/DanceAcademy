#nullable enable
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Api.Endpoints;

public static class MeAdminEndpoints
{
    public static void MapMeAndAdmin(this WebApplication app)
    {
        app.MapGet("/me", [Authorize] (ClaimsPrincipal user) =>
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = user.FindFirstValue(ClaimTypes.Email);
            var role = user.FindFirstValue(ClaimTypes.Role);

            return Results.Ok(new { id, email, role });
        });

        app.MapGet("/admin/ping", [Authorize(Roles = Roles.Admin)] () =>
            Results.Ok(new { ok = true, message = "Admin OK" }));
    }
}