#nullable enable
namespace DanceAcademy.Domain.Entities;

public sealed class Module
{
    private Module() { }

    public Module(Guid courseId, string title, int order)
    {
        if (courseId == Guid.Empty)
            throw new ArgumentException("CourseId es obligatorio.", nameof(courseId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del módulo es obligatorio.", nameof(title));
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "El orden debe ser >= 1.");

        Id = Guid.NewGuid();
        CourseId = courseId;
        Title = title.Trim();
        Order = order;
        IsPublished = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid CourseId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Navegación
    private readonly List<Lesson> _lessons = new();
    public IReadOnlyCollection<Lesson> Lessons => _lessons;

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

    public void UpdateDetails(string title, int order)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del módulo es obligatorio.", nameof(title));
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "El orden debe ser >= 1.");

        Title = title.Trim();
        Order = order;
        Touch();
    }

    public Lesson AddLesson(string title, int order, string? content = null, string? videoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título de la lección es obligatorio.", nameof(title));
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "El orden debe ser >= 1.");

        var lesson = new Lesson(moduleId: Id, title: title.Trim(), order: order, content: content, videoUrl: videoUrl);
        _lessons.Add(lesson);
        Touch();
        return lesson;
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}