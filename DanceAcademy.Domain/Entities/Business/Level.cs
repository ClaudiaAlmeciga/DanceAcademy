#nullable enable
namespace DanceAcademy.Domain.Entities;

public sealed class Level
{
    private Level() { }

    public Level(string name, int order, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del nivel es obligatorio.", nameof(name));
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "El orden debe ser >= 1.");

        Id = Guid.NewGuid();
        Name = name.Trim();
        Order = order;
        IsActive = isActive;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void UpdateDetails(string name, int order)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del nivel es obligatorio.", nameof(name));
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "El orden debe ser >= 1.");

        Name = name.Trim();
        Order = order;
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
