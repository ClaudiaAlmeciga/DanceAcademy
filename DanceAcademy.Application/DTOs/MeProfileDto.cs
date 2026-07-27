#nullable enable
namespace DanceAcademy.Application.DTOs;

public sealed record MeProfileDto(
    Guid Id,
    string Email,
    string Role,
    string? FullName,
    string? Phone,
    DateOnly? BirthDate
);
