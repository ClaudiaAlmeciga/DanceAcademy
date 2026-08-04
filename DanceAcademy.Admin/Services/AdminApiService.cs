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

    public Task<List<AdminLevelDto>?> GetLevelsAsync(CancellationToken ct = default)
        => Client.GetFromJsonAsync<List<AdminLevelDto>>("/admin/levels", ct);

    public Task<AdminLevelDto?> GetLevelDetailAsync(Guid levelId, CancellationToken ct = default)
        => Client.GetFromJsonAsync<AdminLevelDto>($"/admin/levels/{levelId}", ct);

    public Task<HttpResponseMessage> CreateLevelAsync(CreateLevelRequest request, CancellationToken ct = default)
        => Client.PostAsJsonAsync("/admin/levels", request, ct);

    public Task<HttpResponseMessage> UpdateLevelAsync(Guid levelId, UpdateLevelRequest request, CancellationToken ct = default)
        => Client.PutAsJsonAsync($"/admin/levels/{levelId}", request, ct);

    public Task<HttpResponseMessage> DeleteLevelAsync(Guid levelId, CancellationToken ct = default)
        => Client.DeleteAsync($"/admin/levels/{levelId}", ct);

    public Task<HttpResponseMessage> ActivateLevelAsync(Guid levelId, CancellationToken ct = default)
        => Client.PatchAsync($"/admin/levels/{levelId}/activate", null, ct);

    public Task<HttpResponseMessage> DeactivateLevelAsync(Guid levelId, CancellationToken ct = default)
        => Client.PatchAsync($"/admin/levels/{levelId}/deactivate", null, ct);

    public Task<List<AdminSubscriptionPlanDto>?> GetSubscriptionPlansAsync(CancellationToken ct = default)
        => Client.GetFromJsonAsync<List<AdminSubscriptionPlanDto>>("/admin/subscription-plans", ct);

    public Task<AdminSubscriptionPlanDto?> GetSubscriptionPlanDetailAsync(Guid planId, CancellationToken ct = default)
        => Client.GetFromJsonAsync<AdminSubscriptionPlanDto>($"/admin/subscription-plans/{planId}", ct);

    public Task<HttpResponseMessage> CreateSubscriptionPlanAsync(CreateSubscriptionPlanRequest request, CancellationToken ct = default)
        => Client.PostAsJsonAsync("/admin/subscription-plans", request, ct);

    public Task<HttpResponseMessage> UpdateSubscriptionPlanAsync(Guid planId, UpdateSubscriptionPlanRequest request, CancellationToken ct = default)
        => Client.PutAsJsonAsync($"/admin/subscription-plans/{planId}", request, ct);

    public Task<HttpResponseMessage> DeleteSubscriptionPlanAsync(Guid planId, CancellationToken ct = default)
        => Client.DeleteAsync($"/admin/subscription-plans/{planId}", ct);

    public Task<HttpResponseMessage> ActivateSubscriptionPlanAsync(Guid planId, CancellationToken ct = default)
        => Client.PatchAsync($"/admin/subscription-plans/{planId}/activate", null, ct);

    public Task<HttpResponseMessage> DeactivateSubscriptionPlanAsync(Guid planId, CancellationToken ct = default)
        => Client.PatchAsync($"/admin/subscription-plans/{planId}/deactivate", null, ct);

    public Task<AdminDashboardSummaryDto?> GetDashboardSummaryAsync(CancellationToken ct = default)
        => Client.GetFromJsonAsync<AdminDashboardSummaryDto>("/admin/dashboard/summary", ct);

    public Task<List<AdminCourseStatsDto>?> GetCourseStatsAsync(CancellationToken ct = default)
        => Client.GetFromJsonAsync<List<AdminCourseStatsDto>>("/admin/dashboard/courses", ct);

    public Task<List<AdminInstructorDto>?> GetInstructorsAsync(CancellationToken ct = default)
        => Client.GetFromJsonAsync<List<AdminInstructorDto>>("/admin/instructors", ct);

    public Task<AdminInstructorDto?> GetInstructorDetailAsync(Guid instructorId, CancellationToken ct = default)
        => Client.GetFromJsonAsync<AdminInstructorDto>($"/admin/instructors/{instructorId}", ct);

    public Task<HttpResponseMessage> CreateInstructorAsync(CreateInstructorRequest request, CancellationToken ct = default)
        => Client.PostAsJsonAsync("/admin/instructors", request, ct);

    public Task<HttpResponseMessage> UpdateInstructorAsync(Guid instructorId, UpdateInstructorRequest request, CancellationToken ct = default)
        => Client.PutAsJsonAsync($"/admin/instructors/{instructorId}", request, ct);

    public Task<HttpResponseMessage> DeleteInstructorAsync(Guid instructorId, CancellationToken ct = default)
        => Client.DeleteAsync($"/admin/instructors/{instructorId}", ct);

    public Task<List<AdminTestimonialDto>?> GetTestimonialsAsync(CancellationToken ct = default)
        => Client.GetFromJsonAsync<List<AdminTestimonialDto>>("/admin/testimonials", ct);

    public Task<AdminTestimonialDto?> GetTestimonialDetailAsync(Guid testimonialId, CancellationToken ct = default)
        => Client.GetFromJsonAsync<AdminTestimonialDto>($"/admin/testimonials/{testimonialId}", ct);

    public Task<HttpResponseMessage> PublishTestimonialAsync(Guid testimonialId, CancellationToken ct = default)
        => Client.PatchAsync($"/admin/testimonials/{testimonialId}/publish", null, ct);

    public Task<HttpResponseMessage> UnpublishTestimonialAsync(Guid testimonialId, CancellationToken ct = default)
        => Client.PatchAsync($"/admin/testimonials/{testimonialId}/unpublish", null, ct);

    public Task<List<AdminFaqItemDto>?> GetFaqItemsAsync(CancellationToken ct = default)
        => Client.GetFromJsonAsync<List<AdminFaqItemDto>>("/admin/faq", ct);

    public Task<AdminFaqItemDto?> GetFaqItemDetailAsync(Guid faqItemId, CancellationToken ct = default)
        => Client.GetFromJsonAsync<AdminFaqItemDto>($"/admin/faq/{faqItemId}", ct);

    public Task<HttpResponseMessage> CreateFaqItemAsync(CreateFaqItemRequest request, CancellationToken ct = default)
        => Client.PostAsJsonAsync("/admin/faq", request, ct);

    public Task<HttpResponseMessage> UpdateFaqItemAsync(Guid faqItemId, UpdateFaqItemRequest request, CancellationToken ct = default)
        => Client.PutAsJsonAsync($"/admin/faq/{faqItemId}", request, ct);

    public Task<HttpResponseMessage> DeleteFaqItemAsync(Guid faqItemId, CancellationToken ct = default)
        => Client.DeleteAsync($"/admin/faq/{faqItemId}", ct);

    public Task<List<AdminStudentListItemDto>?> GetStudentsAsync(CancellationToken ct = default)
        => Client.GetFromJsonAsync<List<AdminStudentListItemDto>>("/admin/students", ct);

    public Task<AdminStudentDetailDto?> GetStudentDetailAsync(Guid studentId, CancellationToken ct = default)
        => Client.GetFromJsonAsync<AdminStudentDetailDto>($"/admin/students/{studentId}", ct);
}
