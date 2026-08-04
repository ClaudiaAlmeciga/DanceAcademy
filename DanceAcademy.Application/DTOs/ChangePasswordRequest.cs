#nullable enable
namespace DanceAcademy.Application.DTOs;

public sealed record ChangePasswordRequest(
    string? CurrentPassword,
    string? NewPassword
);
