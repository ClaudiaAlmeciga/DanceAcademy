#nullable enable
using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Application.DTOs.Public;

public sealed record CourseListItemDto(
    Guid Id,
    string Title,
    string? Description,
    Guid LevelId,
    string LevelName,
    PricingType PricingType,
    decimal? Price
);

public sealed record ModuleDto(
    Guid Id,
    string Title,
    int Order,
    IReadOnlyList<LessonDto> Lessons
);

public sealed record CourseDetailDto(
    Guid Id,
    string Title,
    string? Description,
    Guid LevelId,
    string LevelName,
    PricingType PricingType,
    decimal? Price,
    IReadOnlyList<SubscriptionPlanDto> SubscriptionPlans,
    IReadOnlyList<ModuleDto> Modules
);

public sealed record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int BillingPeriodDays
);

public sealed record LevelDto(
    Guid Id,
    string Name
);

public sealed record LessonDto(
    Guid Id,
    string Title,
    int Order,
    bool HasVideo
);

public sealed record LessonDetailDto(
    Guid Id,
    Guid ModuleId,
    Guid CourseId,
    string Title,
    string? Content,
    string? VideoUrl,
    string? EmbedUrl,
    bool IsDirectVideo,
    bool RequiresEnrollment
);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int Total
);
