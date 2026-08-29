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
    int LessonCount,
    string? ImageUrl,
    int? DurationHours
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
    IReadOnlyList<AdminModuleDto> Modules,
    string? ImageUrl,
    int? DurationHours
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
    double AverageCompletionRate,
    int TotalInstructors,
    int TotalTestimonials
);

public sealed record AdminCourseStatsDto(
    Guid CourseId,
    string CourseTitle,
    int EnrollmentCount,
    double AverageCompletionRate
);

public sealed record AdminInstructorDto(
    Guid Id,
    string FullName,
    string Specialty,
    string? Bio,
    string? PhotoUrl,
    bool IsActive
);

public sealed record AdminTestimonialDto(
    Guid Id,
    string StudentName,
    string Content,
    int Rating,
    Guid? CourseId,
    string? CourseTitle,
    string? PhotoUrl,
    bool IsPublished
);

public sealed record AdminFaqItemDto(
    Guid Id,
    string Question,
    string Answer,
    string Category,
    int Order,
    bool IsActive
);

public sealed record AdminStudentCourseProgressDto(
    Guid CourseId,
    string CourseTitle,
    DateTimeOffset EnrolledAt,
    int TotalLessons,
    int CompletedLessons,
    double ProgressPercentage
);

public sealed record AdminStudentListItemDto(
    Guid Id,
    string Email,
    string? FullName,
    bool IsActive,
    DateTime CreatedAt,
    int EnrolledCoursesCount,
    double AverageProgressPercentage
);

public sealed record AdminStudentDetailDto(
    Guid Id,
    string Email,
    string? FullName,
    string? Phone,
    bool IsActive,
    DateTime CreatedAt,
    IReadOnlyList<AdminStudentCourseProgressDto> Courses
);

public sealed record AdminEventDto(
    Guid Id,
    string Title,
    string? Description,
    string? Location,
    DateTimeOffset EventDate,
    decimal Price,
    int Capacity,
    int RegisteredCount,
    string? ImageUrl,
    bool IsPublished
);

public sealed record AdminEventRegistrationDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    string? UserFullName,
    EventRegistrationStatus Status,
    DateTimeOffset RegisteredAt,
    DateTimeOffset? PaidAt
);

public sealed record AdminNewsPostDto(
    Guid Id,
    string Title,
    string Content,
    string? ImageUrl,
    DateTimeOffset PublishedAt,
    bool IsPublished
);
