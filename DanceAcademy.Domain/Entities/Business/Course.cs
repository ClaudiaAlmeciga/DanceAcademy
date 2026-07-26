#nullable enable
namespace DanceAcademy.Domain.Entities;

public sealed class Course
{
    // EF Core requiere constructor sin parámetros 
    private Course() { }

    public Course(
        string title,
        string? description = null,
        CourseLevel level = CourseLevel.Beginner,
        bool isPublished = false)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del curso es obligatorio.", nameof(title));

        Id = Guid.NewGuid();
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Level = level;
        IsPublished = isPublished;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public CourseLevel Level { get; private set; } = CourseLevel.Beginner;
    public bool IsPublished { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Navegación
    private readonly List<Module> _modules = new();
    public IReadOnlyCollection<Module> Modules => _modules;

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

    public void SetLevel(CourseLevel level)
    {
        Level = level;
        Touch();
    }

    public void UpdateDetails(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del curso es obligatorio.", nameof(title));

        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Touch();
    }

    public Module AddModule(string title, int order)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del módulo es obligatorio.", nameof(title));
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "El orden debe ser >= 1.");

        var module = new Module(courseId: Id, title: title.Trim(), order: order);
        _modules.Add(module);
        Touch();
        return module;
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

}
