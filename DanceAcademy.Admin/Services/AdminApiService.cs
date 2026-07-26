using DanceAcademy.Application.DTOs.Admin;
using System.Net.Http.Json;

namespace DanceAcademy.Admin.Services;

public class AdminApiService(IHttpClientFactory factory)
{
    private HttpClient Client => factory.CreateClient("api");

    public Task<List<AdminCourseListItemDto>?> GetCoursesAsync(CancellationToken ct = default)
        => Client.GetFromJsonAsync<List<AdminCourseListItemDto>>("/admin/courses", ct);

    public Task<AdminCourseDetailDto?> GetCourseDetailAsync(Guid courseId, CancellationToken ct = default)
        => Client.GetFromJsonAsync<AdminCourseDetailDto>($"/admin/courses/{courseId}", ct);

    public Task<HttpResponseMessage> CreateCourseAsync(CreateCourseRequest request, CancellationToken ct = default)
        => Client.PostAsJsonAsync("/admin/courses", request, ct);

    public Task<HttpResponseMessage> PublishCourseAsync(Guid courseId, CancellationToken ct = default)
        => Client.PatchAsync($"/admin/courses/{courseId}/publish", null, ct);

    public Task<HttpResponseMessage> UnpublishCourseAsync(Guid courseId, CancellationToken ct = default)
        => Client.PatchAsync($"/admin/courses/{courseId}/unpublish", null, ct);

    public Task<HttpResponseMessage> CreateModuleAsync(Guid courseId, CreateModuleRequest request, CancellationToken ct = default)
        => Client.PostAsJsonAsync($"/admin/courses/{courseId}/modules", request, ct);

    public Task<HttpResponseMessage> PublishModuleAsync(Guid moduleId, CancellationToken ct = default)
        => Client.PatchAsync($"/admin/modules/{moduleId}/publish", null, ct);

    public Task<HttpResponseMessage> UnpublishModuleAsync(Guid moduleId, CancellationToken ct = default)
        => Client.PatchAsync($"/admin/modules/{moduleId}/unpublish", null, ct);

    public Task<HttpResponseMessage> CreateLessonAsync(Guid moduleId, CreateLessonRequest request, CancellationToken ct = default)
        => Client.PostAsJsonAsync($"/admin/modules/{moduleId}/lessons", request, ct);

    public Task<HttpResponseMessage> PublishLessonAsync(Guid lessonId, CancellationToken ct = default)
        => Client.PatchAsync($"/admin/lessons/{lessonId}/publish", null, ct);

    public Task<HttpResponseMessage> UnpublishLessonAsync(Guid lessonId, CancellationToken ct = default)
        => Client.PatchAsync($"/admin/lessons/{lessonId}/unpublish", null, ct);

    public Task<HttpResponseMessage> UpdateCourseAsync(Guid courseId, UpdateCourseRequest request, CancellationToken ct = default)
        => Client.PutAsJsonAsync($"/admin/courses/{courseId}", request, ct);

    public Task<HttpResponseMessage> UpdateModuleAsync(Guid moduleId, UpdateModuleRequest request, CancellationToken ct = default)
        => Client.PutAsJsonAsync($"/admin/modules/{moduleId}", request, ct);

    public Task<HttpResponseMessage> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request, CancellationToken ct = default)
        => Client.PutAsJsonAsync($"/admin/lessons/{lessonId}", request, ct);

    public Task<HttpResponseMessage> DeleteCourseAsync(Guid courseId, CancellationToken ct = default)
        => Client.DeleteAsync($"/admin/courses/{courseId}", ct);

    public Task<HttpResponseMessage> DeleteModuleAsync(Guid moduleId, CancellationToken ct = default)
        => Client.DeleteAsync($"/admin/modules/{moduleId}", ct);

    public Task<HttpResponseMessage> DeleteLessonAsync(Guid lessonId, CancellationToken ct = default)
        => Client.DeleteAsync($"/admin/lessons/{lessonId}", ct);
}
