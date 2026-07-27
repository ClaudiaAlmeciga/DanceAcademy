using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Application.DTOs.Admin;

public sealed record CreateCourseRequest(string Title, Guid LevelId, string? Description, PricingType PricingType, decimal? Price, IReadOnlyList<Guid> SubscriptionPlanIds);
public sealed record UpdateCourseRequest(string Title, Guid LevelId, string? Description, PricingType PricingType, decimal? Price, IReadOnlyList<Guid> SubscriptionPlanIds);

public sealed record CreateModuleRequest(string Title, int Order);
public sealed record UpdateModuleRequest(string Title, int Order);

public sealed record CreateLessonRequest(string Title, int Order, string? Content, string? VideoUrl);
public sealed record UpdateLessonRequest(string Title, int Order, string? Content, string? VideoUrl);

public sealed record CreateLevelRequest(string Name, int Order);
public sealed record UpdateLevelRequest(string Name, int Order, bool IsActive);

public sealed record CreateSubscriptionPlanRequest(string Name, string? Description, decimal Price, int BillingPeriodDays);
public sealed record UpdateSubscriptionPlanRequest(string Name, string? Description, decimal Price, int BillingPeriodDays, bool IsActive);
