#nullable enable
namespace DanceAcademy.Domain.Entities;

public sealed class Lesson
{
    private Lesson() { }

    public Lesson(Guid moduleId, string title, int order, string? content, string? videoUrl)
    {
        if (moduleId == Guid.Empty)
            throw new ArgumentException("ModuleId es obligatorio.", nameof(moduleId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título de la lección es obligatorio.", nameof(title));
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "El orden debe ser >= 1.");

        Id = Guid.NewGuid();
        ModuleId = moduleId;
        Title = title.Trim();
        Order = order;
        Content = string.IsNullOrWhiteSpace(content) ? null : content.Trim();
        VideoUrl = string.IsNullOrWhiteSpace(videoUrl) ? null : videoUrl.Trim();
        IsPublished = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Lesson Create(Guid moduleId, string title, int order, string? content, string? videoUrl)
    => new(moduleId, title, order, content, videoUrl);

    public Guid Id { get; private set; }
    public Guid ModuleId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public string? Content { get; private set; }
    public string? VideoUrl { get; private set; }

    public bool IsPublished { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

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

    public void UpdateDetails(string title, int order, string? content, string? videoUrl)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título de la lección es obligatorio.", nameof(title));
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "El orden debe ser >= 1.");

        Title = title.Trim();
        Order = order;
        Content = string.IsNullOrWhiteSpace(content) ? null : content.Trim();
        VideoUrl = string.IsNullOrWhiteSpace(videoUrl) ? null : videoUrl.Trim();
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
