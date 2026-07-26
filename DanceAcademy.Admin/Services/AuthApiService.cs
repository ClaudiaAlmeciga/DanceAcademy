using DanceAcademy.Admin.Auth;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace DanceAcademy.Admin.Services;

public class AuthApiService(
    IHttpClientFactory factory,
    TokenStorageService tokenStorage,
    JwtAuthStateProvider authStateProvider)
{
    public async Task<(bool Success, string? Error)> LoginAsync(
        string email, string password, CancellationToken ct = default)
    {
        var client = factory.CreateClient("api");
        var response = await client.PostAsJsonAsync("/auth/login", new { email, password }, ct);

        if (!response.IsSuccessStatusCode)
            return (false, "Email o contraseña incorrectos.");

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
        if (result?.AccessToken is null)
            return (false, "Respuesta inválida del servidor.");

        await tokenStorage.SaveAsync(result.AccessToken);
        authStateProvider.NotifyStateChanged(result.AccessToken);
        return (true, null);
    }

    public async Task LogoutAsync()
    {
        await tokenStorage.RemoveAsync();
        authStateProvider.NotifyStateChanged(null);
    }

    private sealed record LoginResponse(
        [property: JsonPropertyName("access_token")] string AccessToken
    );
}
