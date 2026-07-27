using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Tests.Domain;

public class ModuleTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesUnpublishedModule()
    {
        var module = new Module(Guid.NewGuid(), "Módulo 1", 1);

        Assert.False(module.IsPublished);
        Assert.Equal(1, module.Order);
    }

    [Fact]
    public void Constructor_WithEmptyCourseId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Module(Guid.Empty, "Módulo 1", 1));
    }

    [Fact]
    public void Constructor_WithEmptyTitle_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Module(Guid.NewGuid(), "", 1));
    }

    [Fact]
    public void Constructor_WithOrderLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Module(Guid.NewGuid(), "Módulo 1", 0));
    }

    [Fact]
    public void Publish_WhenCalled_SetsIsPublishedToTrue()
    {
        var module = new Module(Guid.NewGuid(), "Módulo 1", 1);

        module.Publish();

        Assert.True(module.IsPublished);
    }

    [Fact]
    public void Unpublish_WhenCalled_SetsIsPublishedToFalse()
    {
        var module = new Module(Guid.NewGuid(), "Módulo 1", 1);
        module.Publish();

        module.Unpublish();

        Assert.False(module.IsPublished);
    }

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesTitleAndOrder()
    {
        var module = new Module(Guid.NewGuid(), "Módulo 1", 1);

        module.UpdateDetails("Módulo actualizado", 2);

        Assert.Equal("Módulo actualizado", module.Title);
        Assert.Equal(2, module.Order);
    }

    [Fact]
    public void AddLesson_WithValidData_AddsLessonToCollection()
    {
        var module = new Module(Guid.NewGuid(), "Módulo 1", 1);

        var lesson = module.AddLesson("Lección 1", 1);

        Assert.Single(module.Lessons);
        Assert.Equal(module.Id, lesson.ModuleId);
    }

    [Fact]
    public void AddLesson_WithEmptyTitle_ThrowsArgumentException()
    {
        var module = new Module(Guid.NewGuid(), "Módulo 1", 1);

        Assert.Throws<ArgumentException>(() => module.AddLesson("", 1));
    }
}
