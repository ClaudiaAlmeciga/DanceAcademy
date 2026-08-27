#nullable enable
namespace DanceAcademy.Domain.Entities;

public sealed class Event
{
    // EF Core requiere constructor sin parámetros
    private Event() { }

    public Event(
        string title,
        string? description,
        string? location,
        DateTimeOffset eventDate,
        decimal price,
        int capacity,
        string? imageUrl = null,
        bool isPublished = false)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del evento es obligatorio.", nameof(title));
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "El precio no puede ser negativo.");
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), "El cupo debe ser al menos 1.");

        Id = Guid.NewGuid();
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        EventDate = eventDate.ToUniversalTime();
        Price = price;
        Capacity = capacity;
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        IsPublished = isPublished;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Location { get; private set; }
    public DateTimeOffset EventDate { get; private set; }
    public decimal Price { get; private set; }
    public int Capacity { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void UpdateDetails(string title, string? description, string? location, DateTimeOffset eventDate, decimal price, int capacity, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del evento es obligatorio.", nameof(title));
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "El precio no puede ser negativo.");
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), "El cupo debe ser al menos 1.");

        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        EventDate = eventDate.ToUniversalTime();
        Price = price;
        Capacity = capacity;
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        Touch();
    }

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
