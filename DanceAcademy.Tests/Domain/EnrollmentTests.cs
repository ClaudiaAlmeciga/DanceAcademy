using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Tests.Domain;

public class EnrollmentTests
{
    [Fact]
    public void Constructor_WithValidData_SetsEnrolledAtToUtcNow()
    {
        var before = DateTimeOffset.UtcNow;

        var enrollment = new Enrollment(Guid.NewGuid(), Guid.NewGuid());

        Assert.True(enrollment.EnrolledAt >= before);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Enrollment(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_WithEmptyCourseId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Enrollment(Guid.NewGuid(), Guid.Empty));
    }
}
