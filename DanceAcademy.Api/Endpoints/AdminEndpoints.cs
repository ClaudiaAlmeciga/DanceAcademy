#nullable enable
using Microsoft.AspNetCore.Authorization;

namespace DanceAcademy.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/ping", [Authorize(Roles = Roles.Admin)] () =>
            Results.Ok(new { ok = true, message = "Admin OK" }));
    }
}