#nullable enable
namespace DanceAcademy.Domain.Entities;

/// <summary>
/// Publicación de noticias/actividades realizadas, visible en el sitio público una vez
/// publicada. Redactada y moderada solo por el Admin (a diferencia de los testimonios).
/// </summary>
public sealed class NewsPost
{
    // EF Core requiere constructor sin parámetros
    private NewsPost() { }

    public NewsPost(string title, string content, string? imageUrl, DateTimeOffset publishedAt, bool isPublished = false)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título de la noticia es obligatorio.", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("El contenido de la noticia es obligatorio.", nameof(content));

        Id = Guid.NewGuid();
        Title = title.Trim();
        Content = content.Trim();
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        PublishedAt = publishedAt.ToUniversalTime();
        IsPublished = isPublished;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public DateTimeOffset PublishedAt { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void UpdateDetails(string title, string content, string? imageUrl, DateTimeOffset publishedAt)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título de la noticia es obligatorio.", nameof(title));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("El contenido de la noticia es obligatorio.", nameof(content));

        Title = title.Trim();
        Content = content.Trim();
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        PublishedAt = publishedAt.ToUniversalTime();
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
