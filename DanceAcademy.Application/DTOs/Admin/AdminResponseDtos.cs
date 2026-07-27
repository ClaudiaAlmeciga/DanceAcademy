using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Application.DTOs.Admin;

public sealed record AdminLessonDto(
    Guid Id,
    string Title,
    int Order,
    bool IsPublished,
    string? VideoUrl,
    string? Content
);

public sealed record AdminModuleDto(
    Guid Id,
    string Title,
    int Order,
    bool IsPublished,
    IReadOnlyList<AdminLessonDto> Lessons
);

public sealed record AdminCourseListItemDto(
    Guid Id,
    string Title,
    string? Description,
    Guid LevelId,
    string LevelName,
    bool IsPublished,
    PricingType PricingType,
    decimal? Price,
    int ModuleCount,
    int LessonCount
);

public sealed record AdminCourseDetailDto(
    Guid Id,
    string Title,
    string? Description,
    Guid LevelId,
    string LevelName,
    bool IsPublished,
    PricingType PricingType,
    decimal? Price,
    IReadOnlyList<Guid> SubscriptionPlanIds,
    IReadOnlyList<AdminModuleDto> Modules
);

public sealed record AdminLevelDto(
    Guid Id,
    string Name,
    int Order,
    bool IsActive,
    int CourseCount
);

public sealed record AdminSubscriptionPlanDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int BillingPeriodDays,
    bool IsActive
);

public sealed record AdminDashboardSummaryDto(
    int TotalStudents,
    int TotalPublishedCourses,
    int TotalEnrollments,
    double AverageCompletionRate
);

public sealed record AdminCourseStatsDto(
    Guid CourseId,
    string CourseTitle,
    int EnrollmentCount,
    double AverageCompletionRate
);
