namespace DanceAcademy.Application.DTOs;

public sealed record ResetPasswordRequest(string? Token, string? NewPassword);
