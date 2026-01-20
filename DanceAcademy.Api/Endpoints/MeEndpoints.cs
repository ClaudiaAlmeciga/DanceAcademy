#nullable enable
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DanceAcademy.Api.Endpoints;

public static class MeEndpoints
{
    public static void MapMeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me", [Authorize] (ClaimsPrincipal user) =>
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = user.FindFirstValue(ClaimTypes.Email);
            var role = user.FindFirstValue(ClaimTypes.Role);

            return Results.Ok(new { id, email, role });
        });
    }
}