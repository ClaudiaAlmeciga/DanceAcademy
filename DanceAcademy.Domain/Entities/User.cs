#nullable enable
using System;

namespace DanceAcademy.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = Roles.Student;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    public string? FullName { get; private set; }
    public string? Phone { get; private set; }
    public DateOnly? BirthDate { get; private set; }

    public void UpdateProfile(string? fullName, string? phone, DateOnly? birthDate)
    {
        FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        BirthDate = birthDate;
    }
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Student = "Student";
}
