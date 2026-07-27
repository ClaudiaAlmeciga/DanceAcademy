using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Tests.Domain;

public class LevelTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesActiveLevel()
    {
        var level = new Level("Principiante", 1);

        Assert.True(level.IsActive);
        Assert.Equal("Principiante", level.Name);
        Assert.Equal(1, level.Order);
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Level("", 1));
    }

    [Fact]
    public void Constructor_WithOrderLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Level("Principiante", 0));
    }

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesNameAndOrder()
    {
        var level = new Level("Principiante", 1);

        level.UpdateDetails("Básico", 2);

        Assert.Equal("Básico", level.Name);
        Assert.Equal(2, level.Order);
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_ThrowsArgumentException()
    {
        var level = new Level("Principiante", 1);

        Assert.Throws<ArgumentException>(() => level.UpdateDetails("", 1));
    }

    [Fact]
    public void Deactivate_WhenCalled_SetsIsActiveToFalse()
    {
        var level = new Level("Principiante", 1);

        level.Deactivate();

        Assert.False(level.IsActive);
    }

    [Fact]
    public void Activate_WhenCalled_SetsIsActiveToTrue()
    {
        var level = new Level("Principiante", 1, isActive: false);

        level.Activate();

        Assert.True(level.IsActive);
    }
}
