using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Tests.Domain;

public class NewsPostTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesUnpublishedPost()
    {
        var publishedAt = DateTimeOffset.UtcNow;

        var post = new NewsPost("Muestra de fin de año", "Contenido de la noticia", "https://example.com/img.jpg", publishedAt);

        Assert.False(post.IsPublished);
        Assert.Equal("Muestra de fin de año", post.Title);
        Assert.Equal("Contenido de la noticia", post.Content);
        Assert.Equal("https://example.com/img.jpg", post.ImageUrl);
        Assert.Equal(publishedAt, post.PublishedAt);
    }

    [Fact]
    public void Constructor_WithEmptyTitle_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new NewsPost("", "Contenido", null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Constructor_WithEmptyContent_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new NewsPost("Título", "", null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Constructor_WithNullImageUrl_CreatesPostWithNullImageUrl()
    {
        var post = new NewsPost("Título", "Contenido", null, DateTimeOffset.UtcNow);

        Assert.Null(post.ImageUrl);
    }

    [Fact]
    public void Publish_WhenCalled_SetsIsPublishedToTrue()
    {
        var post = new NewsPost("Título", "Contenido", null, DateTimeOffset.UtcNow);

        post.Publish();

        Assert.True(post.IsPublished);
    }

    [Fact]
    public void Unpublish_WhenCalled_SetsIsPublishedToFalse()
    {
        var post = new NewsPost("Título", "Contenido", null, DateTimeOffset.UtcNow, isPublished: true);

        post.Unpublish();

        Assert.False(post.IsPublished);
    }

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesFields()
    {
        var post = new NewsPost("Título", "Contenido", null, DateTimeOffset.UtcNow);
        var newDate = DateTimeOffset.UtcNow.AddDays(-3);

        post.UpdateDetails("Nuevo título", "Nuevo contenido", "https://example.com/nueva.jpg", newDate);

        Assert.Equal("Nuevo título", post.Title);
        Assert.Equal("Nuevo contenido", post.Content);
        Assert.Equal("https://example.com/nueva.jpg", post.ImageUrl);
        Assert.Equal(newDate, post.PublishedAt);
    }

    [Fact]
    public void UpdateDetails_WithEmptyTitle_ThrowsArgumentException()
    {
        var post = new NewsPost("Título", "Contenido", null, DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(() => post.UpdateDetails("", "Contenido", null, DateTimeOffset.UtcNow));
    }
}
