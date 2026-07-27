#nullable enable
namespace DanceAcademy.Application.DTOs;

public sealed record EnrollmentDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    DateTimeOffset EnrolledAt
);

public sealed record CreateEnrollmentRequest(
    Guid CourseId
);
