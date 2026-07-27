#nullable enable
namespace DanceAcademy.Application.DTOs;

public sealed record UpdateProfileRequest(
    string? FullName,
    string? Phone,
    DateOnly? BirthDate
);
