using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Tests.Domain;

public class LessonProgressTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesIncompleteProgress()
    {
        var progress = new LessonProgress(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(progress.IsCompleted);
        Assert.Null(progress.CompletedAt);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new LessonProgress(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_WithEmptyLessonId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new LessonProgress(Guid.NewGuid(), Guid.Empty));
    }

    [Fact]
    public void MarkCompleted_WhenNotCompleted_SetsIsCompletedAndCompletedAt()
    {
        var progress = new LessonProgress(Guid.NewGuid(), Guid.NewGuid());

        progress.MarkCompleted();

        Assert.True(progress.IsCompleted);
        Assert.NotNull(progress.CompletedAt);
    }

    [Fact]
    public void MarkCompleted_WhenAlreadyCompleted_DoesNotChangeOriginalCompletedAt()
    {
        var progress = new LessonProgress(Guid.NewGuid(), Guid.NewGuid());
        progress.MarkCompleted();
        var originalCompletedAt = progress.CompletedAt;

        progress.MarkCompleted();

        Assert.Equal(originalCompletedAt, progress.CompletedAt);
    }

    [Fact]
    public void MarkIncomplete_WhenCompleted_ClearsIsCompletedAndCompletedAt()
    {
        var progress = new LessonProgress(Guid.NewGuid(), Guid.NewGuid());
        progress.MarkCompleted();

        progress.MarkIncomplete();

        Assert.False(progress.IsCompleted);
        Assert.Null(progress.CompletedAt);
    }
}
