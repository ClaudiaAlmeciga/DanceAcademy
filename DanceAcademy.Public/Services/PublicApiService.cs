using DanceAcademy.Application.DTOs.Public;
using DanceAcademy.Public.Auth;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace DanceAcademy.Public.Services;

public class PublicApiService(
    IHttpClientFactory factory,
    TokenStorageService tokenStorage,
    JwtAuthStateProvider authStateProvider)
{
    private HttpClient Client => factory.CreateClient("api");

    public async Task<(bool Success, string? Error)> LoginAsync(
        string email, string password, CancellationToken ct = default)
    {
        var response = await Client.PostAsJsonAsync("/auth/login", new { email, password }, ct);

        if (!response.IsSuccessStatusCode)
            return (false, "Email o contraseña incorrectos.");

        var result = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
        if (result?.AccessToken is null)
            return (false, "Respuesta inválida del servidor.");

        await tokenStorage.SaveAsync(result.AccessToken);
        authStateProvider.NotifyStateChanged(result.AccessToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(
        string email, string password, CancellationToken ct = default)
    {
        var response = await Client.PostAsJsonAsync("/auth/register", new { email, password }, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            return (false, "Ya existe una cuenta con ese email.");

        if (!response.IsSuccessStatusCode)
            return (false, "No se pudo crear la cuenta. Intenta de nuevo.");

        return await LoginAsync(email, password, ct);
    }

    public async Task<PagedResult<CourseListItemDto>?> GetCoursesAsync(
        int page = 1, int pageSize = 12, string? level = null, CancellationToken ct = default)
    {
        var url = $"/public/courses?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(level))
            url += $"&level={Uri.EscapeDataString(level)}";

        var response = await Client.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<PagedResult<CourseListItemDto>>(cancellationToken: ct);
    }

    public async Task<(bool Success, string? Error)> ForgotPasswordAsync(
        string email, CancellationToken ct = default)
    {
        var response = await Client.PostAsJsonAsync("/auth/forgot-password", new { email }, ct);
        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, "No se pudo procesar la solicitud. Intenta de nuevo.");
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(
        string token, string newPassword, CancellationToken ct = default)
    {
        var response = await Client.PostAsJsonAsync("/auth/reset-password",
            new { token, newPassword }, ct);

        if (response.IsSuccessStatusCode)
            return (true, null);

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var body = await response.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken: ct);
            return (false, body?.Error ?? "El enlace es inválido o ha expirado.");
        }

        return (false, "No se pudo restablecer la contraseña. Intenta de nuevo.");
    }

    public async Task LogoutAsync()
    {
        await tokenStorage.RemoveAsync();
        authStateProvider.NotifyStateChanged(null);
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken
    );

    private sealed record ErrorResponse(
        [property: JsonPropertyName("error")] string? Error
    );
}
