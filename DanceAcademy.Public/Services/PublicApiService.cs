using DanceAcademy.Application.DTOs;
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
        int page = 1, int pageSize = 12, Guid? levelId = null, CancellationToken ct = default)
    {
        var url = $"/public/courses?page={page}&pageSize={pageSize}";
        if (levelId is not null)
            url += $"&levelId={levelId}";

        var response = await Client.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<PagedResult<CourseListItemDto>>(cancellationToken: ct);
    }

    public async Task<List<LevelDto>?> GetLevelsAsync(CancellationToken ct = default)
    {
        var response = await Client.GetAsync("/public/levels", ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<List<LevelDto>>(cancellationToken: ct);
    }

    public async Task<CourseDetailDto?> GetCourseDetailAsync(Guid courseId, CancellationToken ct = default)
    {
        var response = await Client.GetAsync($"/public/courses/{courseId}", ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<CourseDetailDto>(cancellationToken: ct);
    }

    public async Task<LessonDetailDto?> GetLessonDetailAsync(Guid lessonId, CancellationToken ct = default)
    {
        var response = await Client.GetAsync($"/public/lessons/{lessonId}", ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<LessonDetailDto>(cancellationToken: ct);
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

    public async Task<List<SubscriptionPlanDto>?> GetSubscriptionPlansAsync(CancellationToken ct = default)
    {
        var response = await Client.GetAsync("/public/subscription-plans", ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<List<SubscriptionPlanDto>>(cancellationToken: ct);
    }

    public async Task<MeProfileDto?> GetProfileAsync(CancellationToken ct = default)
    {
        var response = await Client.GetAsync("/me", ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<MeProfileDto>(cancellationToken: ct);
    }

    public async Task<(bool Success, string? Error)> UpdateProfileAsync(
        UpdateProfileRequest request, CancellationToken ct = default)
    {
        var response = await Client.PutAsJsonAsync("/me", request, ct);
        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, "No se pudo guardar el perfil. Intenta de nuevo.");
    }

    public async Task LogoutAsync()
    {
        await tokenStorage.RemoveAsync();
        authStateProvider.NotifyStateChanged(null);
    }

    public async Task<List<EnrollmentDto>?> GetMyEnrollmentsAsync(CancellationToken ct = default)
    {
        var response = await Client.GetAsync("/me/enrollments", ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<List<EnrollmentDto>>(cancellationToken: ct);
    }

    public async Task<(bool Success, string? Error)> EnrollInCourseAsync(Guid courseId, CancellationToken ct = default)
    {
        var response = await Client.PostAsJsonAsync("/me/enrollments", new CreateEnrollmentRequest(courseId), ct);

        if (response.IsSuccessStatusCode)
            return (true, null);

        try
        {
            var body = await response.Content.ReadFromJsonAsync<MessageResponse>(cancellationToken: ct);
            if (body?.Message is not null)
                return (false, body.Message);
        }
        catch (System.Text.Json.JsonException)
        {
            // El cuerpo de la respuesta de error no era JSON — se usa el mensaje genérico.
        }

        return (false, "No se pudo completar la inscripción. Intenta de nuevo.");
    }

    public async Task<LessonProgressDto?> GetLessonProgressAsync(Guid lessonId, CancellationToken ct = default)
    {
        var response = await Client.GetAsync($"/me/progress/lessons/{lessonId}", ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<LessonProgressDto>(cancellationToken: ct);
    }

    public async Task<(bool Success, string? Error)> MarkLessonCompleteAsync(Guid lessonId, CancellationToken ct = default)
    {
        var response = await Client.PostAsync($"/me/progress/lessons/{lessonId}/complete", null, ct);
        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, "No se pudo registrar el avance. Intenta de nuevo.");
    }

    public async Task<(bool Success, string? Error)> UnmarkLessonCompleteAsync(Guid lessonId, CancellationToken ct = default)
    {
        var response = await Client.DeleteAsync($"/me/progress/lessons/{lessonId}/complete", ct);
        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, "No se pudo actualizar el avance. Intenta de nuevo.");
    }

    public async Task<MyDashboardDto?> GetMyDashboardAsync(CancellationToken ct = default)
    {
        var response = await Client.GetAsync("/me/dashboard", ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<MyDashboardDto>(cancellationToken: ct);
    }

    public async Task<CourseProgressDto?> GetCourseProgressAsync(Guid courseId, CancellationToken ct = default)
    {
        var response = await Client.GetAsync($"/me/progress/courses/{courseId}", ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<CourseProgressDto>(cancellationToken: ct);
    }

    public async Task<List<LessonProgressListItemDto>?> GetCourseLessonProgressAsync(Guid courseId, CancellationToken ct = default)
    {
        var response = await Client.GetAsync($"/me/progress/courses/{courseId}/lessons", ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<List<LessonProgressListItemDto>>(cancellationToken: ct);
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken
    );

    private sealed record ErrorResponse(
        [property: JsonPropertyName("error")] string? Error
    );

    private sealed record MessageResponse(
        [property: JsonPropertyName("message")] string? Message
    );
}
