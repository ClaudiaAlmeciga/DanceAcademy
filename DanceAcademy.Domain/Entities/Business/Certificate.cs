#nullable enable
namespace DanceAcademy.Domain.Entities;

/// <summary>
/// Certificado de finalización, emitido una única vez cuando un estudiante
/// completa el 100% de las lecciones publicadas de un curso.
/// </summary>
public sealed class Certificate
{
    private Certificate() { }

    public Certificate(Guid userId, Guid courseId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId es obligatorio.", nameof(userId));
        if (courseId == Guid.Empty)
            throw new ArgumentException("CourseId es obligatorio.", nameof(courseId));

        Id = Guid.NewGuid();
        UserId = userId;
        CourseId = courseId;
        IssuedAt = DateTimeOffset.UtcNow;
        VerificationCode = $"DA-{Guid.NewGuid():N}"[..11].ToUpperInvariant();
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CourseId { get; private set; }
    public DateTimeOffset IssuedAt { get; private set; }
    public string VerificationCode { get; private set; } = string.Empty;
}
