using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Tests.Domain;

public class LessonTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesUnpublishedLesson()
    {
        var lesson = new Lesson(Guid.NewGuid(), "Lección 1", 1, "Contenido", "https://youtu.be/abc123");

        Assert.False(lesson.IsPublished);
        Assert.Equal("Contenido", lesson.Content);
        Assert.Equal("https://youtu.be/abc123", lesson.VideoUrl);
    }

    [Fact]
    public void Constructor_WithEmptyModuleId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Lesson(Guid.Empty, "Lección 1", 1, null, null));
    }

    [Fact]
    public void Constructor_WithEmptyTitle_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Lesson(Guid.NewGuid(), "", 1, null, null));
    }

    [Fact]
    public void Constructor_WithOrderLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Lesson(Guid.NewGuid(), "Lección 1", 0, null, null));
    }

    [Fact]
    public void Constructor_WithWhitespaceVideoUrl_SetsVideoUrlToNull()
    {
        var lesson = new Lesson(Guid.NewGuid(), "Lección 1", 1, null, "   ");

        Assert.Null(lesson.VideoUrl);
    }

    [Fact]
    public void Publish_WhenCalled_SetsIsPublishedToTrue()
    {
        var lesson = new Lesson(Guid.NewGuid(), "Lección 1", 1, null, null);

        lesson.Publish();

        Assert.True(lesson.IsPublished);
    }

    [Fact]
    public void Unpublish_WhenCalled_SetsIsPublishedToFalse()
    {
        var lesson = new Lesson(Guid.NewGuid(), "Lección 1", 1, null, null);
        lesson.Publish();

        lesson.Unpublish();

        Assert.False(lesson.IsPublished);
    }

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesFields()
    {
        var lesson = new Lesson(Guid.NewGuid(), "Lección 1", 1, null, null);

        lesson.UpdateDetails("Lección actualizada", 2, "Nuevo contenido", "https://youtu.be/xyz789");

        Assert.Equal("Lección actualizada", lesson.Title);
        Assert.Equal(2, lesson.Order);
        Assert.Equal("Nuevo contenido", lesson.Content);
        Assert.Equal("https://youtu.be/xyz789", lesson.VideoUrl);
    }

    [Fact]
    public void UpdateDetails_WithEmptyTitle_ThrowsArgumentException()
    {
        var lesson = new Lesson(Guid.NewGuid(), "Lección 1", 1, null, null);

        Assert.Throws<ArgumentException>(() => lesson.UpdateDetails("", 1, null, null));
    }

    [Fact]
    public void Create_WithValidData_ReturnsLessonInstance()
    {
        var lesson = Lesson.Create(Guid.NewGuid(), "Lección 1", 1, null, null);

        Assert.NotEqual(Guid.Empty, lesson.Id);
    }
}
