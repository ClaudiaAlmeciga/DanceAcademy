using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace DanceAcademy.Public.Auth;

public class JwtAuthStateProvider(TokenStorageService tokenStorage) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenStorage.GetAsync();
        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        var claims = ParseClaimsFromJwt(token).ToList();

        var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
        if (expClaim is not null && long.TryParse(expClaim.Value, out var expSeconds))
        {
            var isExpired = DateTimeOffset.UtcNow >= DateTimeOffset.FromUnixTimeSeconds(expSeconds);
            if (isExpired)
            {
                await tokenStorage.RemoveAsync();
                return Anonymous;
            }
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyStateChanged(string? token)
    {
        ClaimsPrincipal user;
        if (string.IsNullOrWhiteSpace(token))
        {
            user = new ClaimsPrincipal(new ClaimsIdentity());
        }
        else
        {
            var claims = ParseClaimsFromJwt(token);
            user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        }
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var segments = jwt.Split('.');
        if (segments.Length != 3) return [];

        var payload = segments[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var jsonBytes = Convert.FromBase64String(padded);
        var pairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

        if (pairs is null) return [];

        return pairs.Select(kvp =>
        {
            var claimType = kvp.Key switch
            {
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" => ClaimTypes.NameIdentifier,
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"   => ClaimTypes.Email,
                "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"         => ClaimTypes.Role,
                _ => kvp.Key
            };
            var value = kvp.Value.ValueKind == JsonValueKind.String
                ? kvp.Value.GetString()!
                : kvp.Value.GetRawText();
            return new Claim(claimType, value);
        });
    }
}
