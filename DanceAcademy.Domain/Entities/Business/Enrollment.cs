#nullable enable
namespace DanceAcademy.Domain.Entities;

/// <summary>
/// Representa la inscripción de un estudiante a un curso.
/// </summary>
public sealed class Enrollment
{
    // EF Core requiere constructor sin parámetros
    private Enrollment() { }

    public Enrollment(Guid userId, Guid courseId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId es obligatorio.", nameof(userId));
        if (courseId == Guid.Empty)
            throw new ArgumentException("CourseId es obligatorio.", nameof(courseId));

        Id = Guid.NewGuid();
        UserId = userId;
        CourseId = courseId;
        EnrolledAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CourseId { get; private set; }
    public DateTimeOffset EnrolledAt { get; private set; }
}
