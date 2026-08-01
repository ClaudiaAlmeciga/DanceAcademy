#nullable enable
namespace DanceAcademy.Domain.Entities;

public sealed class Instructor
{
    private Instructor() { }

    public Instructor(string fullName, string specialty, string? bio, string? photoUrl, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("El nombre del instructor es obligatorio.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(specialty))
            throw new ArgumentException("La especialidad del instructor es obligatoria.", nameof(specialty));

        Id = Guid.NewGuid();
        FullName = fullName.Trim();
        Specialty = specialty.Trim();
        Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim();
        IsActive = isActive;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Specialty { get; private set; } = string.Empty;
    public string? Bio { get; private set; }
    public string? PhotoUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void UpdateDetails(string fullName, string specialty, string? bio, string? photoUrl)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("El nombre del instructor es obligatorio.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(specialty))
            throw new ArgumentException("La especialidad del instructor es obligatoria.", nameof(specialty));

        FullName = fullName.Trim();
        Specialty = specialty.Trim();
        Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim();
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
