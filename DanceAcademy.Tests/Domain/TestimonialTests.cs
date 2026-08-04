using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Tests.Domain;

public class TestimonialTests
{
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidCourseId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_CreatesUnpublishedTestimonial()
    {
        var testimonial = new Testimonial(ValidUserId, "Andrés Pérez", "Excelente academia", 5, ValidCourseId, "https://example.com/andres.jpg");

        Assert.False(testimonial.IsPublished);
        Assert.Equal(ValidUserId, testimonial.UserId);
        Assert.Equal("Andrés Pérez", testimonial.StudentName);
        Assert.Equal("Excelente academia", testimonial.Content);
        Assert.Equal(5, testimonial.Rating);
        Assert.Equal(ValidCourseId, testimonial.CourseId);
        Assert.Equal("https://example.com/andres.jpg", testimonial.PhotoUrl);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Testimonial(Guid.Empty, "Andrés Pérez", "Excelente academia", 5, null, null));
    }

    [Fact]
    public void Constructor_WithEmptyStudentName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Testimonial(ValidUserId, "", "Excelente academia", 5, null, null));
    }

    [Fact]
    public void Constructor_WithEmptyContent_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Testimonial(ValidUserId, "Andrés Pérez", "", 5, null, null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Constructor_WithRatingOutOfRange_ThrowsArgumentOutOfRangeException(int rating)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Testimonial(ValidUserId, "Andrés Pérez", "Excelente academia", rating, null, null));
    }

    [Fact]
    public void Constructor_WithoutCourseId_CreatesTestimonialWithNullCourseId()
    {
        var testimonial = new Testimonial(ValidUserId, "Andrés Pérez", "Excelente academia", 5, null, null);

        Assert.Null(testimonial.CourseId);
    }

    [Fact]
    public void Publish_WhenCalled_SetsIsPublishedToTrue()
    {
        var testimonial = new Testimonial(ValidUserId, "Andrés Pérez", "Excelente academia", 5, null, null);

        testimonial.Publish();

        Assert.True(testimonial.IsPublished);
    }

    [Fact]
    public void Unpublish_WhenCalled_SetsIsPublishedToFalse()
    {
        var testimonial = new Testimonial(ValidUserId, "Andrés Pérez", "Excelente academia", 5, null, null);
        testimonial.Publish();

        testimonial.Unpublish();

        Assert.False(testimonial.IsPublished);
    }
}
