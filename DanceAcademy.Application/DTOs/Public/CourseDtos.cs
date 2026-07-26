#nullable enable
namespace DanceAcademy.Application.DTOs.Public;

public sealed record CourseListItemDto(
    Guid Id,
    string Title,
    string? Description,
    string Level
);

public sealed record ModuleDto(
    Guid Id,
    string Title,
    int Order
);

public sealed record CourseDetailDto(
    Guid Id,
    string Title,
    string? Description,
    string Level,
    IReadOnlyList<ModuleDto> Modules
);

public sealed record LessonDto(
    Guid Id,
    string Title,
    int Order,
    string? VideoUrl
);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int Total
);
