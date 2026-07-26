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
    string Level,
    bool IsPublished,
    int ModuleCount,
    int LessonCount
);

public sealed record AdminCourseDetailDto(
    Guid Id,
    string Title,
    string? Description,
    string Level,
    bool IsPublished,
    IReadOnlyList<AdminModuleDto> Modules
);
