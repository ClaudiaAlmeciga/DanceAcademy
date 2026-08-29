using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Application.DTOs.Admin;

public sealed record CreateCourseRequest(string Title, Guid LevelId, string? Description, PricingType PricingType, decimal? Price, IReadOnlyList<Guid> SubscriptionPlanIds, string? ImageUrl, int? DurationHours);
public sealed record UpdateCourseRequest(string Title, Guid LevelId, string? Description, PricingType PricingType, decimal? Price, IReadOnlyList<Guid> SubscriptionPlanIds, string? ImageUrl, int? DurationHours);

public sealed record CreateModuleRequest(string Title, int Order);
public sealed record UpdateModuleRequest(string Title, int Order);

public sealed record CreateLessonRequest(string Title, int Order, string? Content, string? VideoUrl);
public sealed record UpdateLessonRequest(string Title, int Order, string? Content, string? VideoUrl);

public sealed record CreateLevelRequest(string Name, int Order);
public sealed record UpdateLevelRequest(string Name, int Order, bool IsActive);

public sealed record CreateSubscriptionPlanRequest(string Name, string? Description, decimal Price, int BillingPeriodDays);
public sealed record UpdateSubscriptionPlanRequest(string Name, string? Description, decimal Price, int BillingPeriodDays, bool IsActive);

public sealed record CreateInstructorRequest(string FullName, string Specialty, string? Bio, string? PhotoUrl);
public sealed record UpdateInstructorRequest(string FullName, string Specialty, string? Bio, string? PhotoUrl, bool IsActive);

// No hay Create ni Update de testimonios desde Admin: solo se crean desde el envío de un
// estudiante autenticado (ver CreateTestimonialSelfRequest en DanceAcademy.Application.DTOs)
// y el Admin únicamente aprueba/rechaza su publicación (PATCH publish/unpublish) — nunca
// redacta ni edita el contenido a nombre de un estudiante.

public sealed record CreateFaqItemRequest(string Question, string Answer, string Category, int Order);
public sealed record UpdateFaqItemRequest(string Question, string Answer, string Category, int Order, bool IsActive);

public sealed record CreateEventRequest(string Title, string? Description, string? Location, DateTimeOffset EventDate, decimal Price, int Capacity, string? ImageUrl);
public sealed record UpdateEventRequest(string Title, string? Description, string? Location, DateTimeOffset EventDate, decimal Price, int Capacity, string? ImageUrl);

public sealed record CreateNewsPostRequest(string Title, string Content, string? ImageUrl, DateTimeOffset PublishedAt);
public sealed record UpdateNewsPostRequest(string Title, string Content, string? ImageUrl, DateTimeOffset PublishedAt);
