#nullable enable
namespace DanceAcademy.Domain.Entities;

public sealed class Testimonial
{
    private Testimonial() { }

    /// <summary>
    /// Los testimonios solo se crean a partir del envío de un estudiante autenticado
    /// (ver <c>MeTestimonialsEndpoints</c>) — nunca redactados por un Admin. Por eso
    /// <paramref name="userId"/> es obligatorio y <see cref="IsPublished"/> siempre
    /// arranca en false: cada envío queda pendiente de moderación antes de ser público.
    /// </summary>
    public Testimonial(Guid userId, string studentName, string content, int rating, Guid? courseId, string? photoUrl)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId es obligatorio.", nameof(userId));
        if (string.IsNullOrWhiteSpace(studentName))
            throw new ArgumentException("El nombre del estudiante es obligatorio.", nameof(studentName));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("El contenido del testimonio es obligatorio.", nameof(content));
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "El rating debe estar entre 1 y 5.");

        Id = Guid.NewGuid();
        UserId = userId;
        StudentName = studentName.Trim();
        Content = content.Trim();
        Rating = rating;
        CourseId = courseId;
        PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim();
        IsPublished = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string StudentName { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public int Rating { get; private set; }
    public Guid? CourseId { get; private set; }
    public string? PhotoUrl { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>
    /// Moderación de Admin: solo puede aprobar o rechazar la publicación — nunca editar
    /// el contenido ni el nombre del estudiante. El testimonio queda tal como lo envió.
    /// </summary>
    public void Publish()
    {
        IsPublished = true;
        Touch();
    }

    public void Unpublish()
    {
        IsPublished = false;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
