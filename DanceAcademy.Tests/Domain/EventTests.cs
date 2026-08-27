using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Tests.Domain;

public class EventTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesUnpublishedEvent()
    {
        var ev = new Event("Noche de Salsa", "Evento abierto", "Salón principal", DateTimeOffset.UtcNow.AddDays(7), 50000m, 30, "https://example.com/salsa.jpg");

        Assert.False(ev.IsPublished);
        Assert.Equal("Noche de Salsa", ev.Title);
        Assert.Equal("Evento abierto", ev.Description);
        Assert.Equal("Salón principal", ev.Location);
        Assert.Equal(50000m, ev.Price);
        Assert.Equal(30, ev.Capacity);
        Assert.Equal("https://example.com/salsa.jpg", ev.ImageUrl);
    }

    [Fact]
    public void Constructor_WithNullImageUrl_CreatesEventWithNullImageUrl()
    {
        var ev = new Event("Evento", null, null, DateTimeOffset.UtcNow, 0m, 10);

        Assert.Null(ev.ImageUrl);
    }

    [Fact]
    public void Constructor_WithEmptyTitle_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Event("", null, null, DateTimeOffset.UtcNow, 0m, 10));
    }

    [Fact]
    public void Constructor_WithNegativePrice_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Event("Evento", null, null, DateTimeOffset.UtcNow, -1m, 10));
    }

    [Fact]
    public void Constructor_WithZeroCapacity_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Event("Evento", null, null, DateTimeOffset.UtcNow, 0m, 0));
    }

    [Fact]
    public void Publish_WhenCalled_SetsIsPublishedToTrue()
    {
        var ev = new Event("Evento", null, null, DateTimeOffset.UtcNow, 0m, 10);

        ev.Publish();

        Assert.True(ev.IsPublished);
    }

    [Fact]
    public void Unpublish_WhenCalled_SetsIsPublishedToFalse()
    {
        var ev = new Event("Evento", null, null, DateTimeOffset.UtcNow, 0m, 10, isPublished: true);

        ev.Unpublish();

        Assert.False(ev.IsPublished);
    }

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesFields()
    {
        var ev = new Event("Evento", null, null, DateTimeOffset.UtcNow, 0m, 10);
        var newDate = DateTimeOffset.UtcNow.AddDays(14);

        ev.UpdateDetails("Evento actualizado", "Nueva descripción", "Nuevo lugar", newDate, 75000m, 50, "https://example.com/nueva.jpg");

        Assert.Equal("Evento actualizado", ev.Title);
        Assert.Equal("Nueva descripción", ev.Description);
        Assert.Equal("Nuevo lugar", ev.Location);
        Assert.Equal(newDate, ev.EventDate);
        Assert.Equal(75000m, ev.Price);
        Assert.Equal(50, ev.Capacity);
        Assert.Equal("https://example.com/nueva.jpg", ev.ImageUrl);
    }

    [Fact]
    public void UpdateDetails_WithZeroCapacity_ThrowsArgumentOutOfRangeException()
    {
        var ev = new Event("Evento", null, null, DateTimeOffset.UtcNow, 0m, 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => ev.UpdateDetails("Evento", null, null, DateTimeOffset.UtcNow, 0m, 0, null));
    }
}
