#nullable enable
namespace DanceAcademy.Domain.Entities;

/// <summary>
/// Representa el avance de un estudiante en una lección específica del curso.
/// </summary>
public sealed class LessonProgress
{
    // EF Core requiere constructor sin parámetros
    private LessonProgress() { }

    public LessonProgress(Guid userId, Guid lessonId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId es obligatorio.", nameof(userId));
        if (lessonId == Guid.Empty)
            throw new ArgumentException("LessonId es obligatorio.", nameof(lessonId));

        Id = Guid.NewGuid();
        UserId = userId;
        LessonId = lessonId;
        IsCompleted = false;
        CompletedAt = null;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid LessonId { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    /// <summary>
    /// Marca la lección como completada por el estudiante. Idempotente: si ya estaba
    /// completada, no modifica la fecha de finalización original.
    /// </summary>
    public void MarkCompleted()
    {
        if (IsCompleted)
            return;

        IsCompleted = true;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Revierte una lección previamente marcada como completada.
    /// </summary>
    public void MarkIncomplete()
    {
        if (!IsCompleted)
            return;

        IsCompleted = false;
        CompletedAt = null;
    }
}
