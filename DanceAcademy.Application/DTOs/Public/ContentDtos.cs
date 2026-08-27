#nullable enable
namespace DanceAcademy.Application.DTOs.Public;

public sealed record InstructorDto(
    Guid Id,
    string FullName,
    string Specialty,
    string? Bio,
    string? PhotoUrl
);

public sealed record TestimonialDto(
    Guid Id,
    string StudentName,
    string Content,
    int Rating,
    string? CourseTitle,
    string? PhotoUrl
);

public sealed record FaqItemDto(
    Guid Id,
    string Question,
    string Answer,
    string Category
);

public sealed record EventDto(
    Guid Id,
    string Title,
    string? Description,
    string? Location,
    DateTimeOffset EventDate,
    decimal Price,
    int Capacity,
    int AvailableSpots,
    string? ImageUrl
);

public sealed record NewsPostDto(
    Guid Id,
    string Title,
    string Content,
    string? ImageUrl,
    DateTimeOffset PublishedAt
);
