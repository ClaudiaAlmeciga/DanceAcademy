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
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Student = "Student";
}
