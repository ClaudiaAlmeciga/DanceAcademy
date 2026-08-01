#nullable enable
namespace DanceAcademy.Application.DTOs;

public sealed record CertificateDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    string VerificationCode,
    DateTimeOffset IssuedAt
);
