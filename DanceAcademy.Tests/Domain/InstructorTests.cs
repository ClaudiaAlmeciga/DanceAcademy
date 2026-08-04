using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Tests.Domain;

public class InstructorTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesActiveInstructor()
    {
        var instructor = new Instructor("Laura Gómez", "Salsa", "Bailarina profesional", "https://example.com/laura.jpg");

        Assert.True(instructor.IsActive);
        Assert.Equal("Laura Gómez", instructor.FullName);
        Assert.Equal("Salsa", instructor.Specialty);
        Assert.Equal("Bailarina profesional", instructor.Bio);
        Assert.Equal("https://example.com/laura.jpg", instructor.PhotoUrl);
    }

    [Fact]
    public void Constructor_WithEmptyFullName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Instructor("", "Salsa", null, null));
    }

    [Fact]
    public void Constructor_WithEmptySpecialty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Instructor("Laura Gómez", "", null, null));
    }

    [Fact]
    public void Constructor_WithNullBioAndPhotoUrl_CreatesInstructorWithNullFields()
    {
        var instructor = new Instructor("Laura Gómez", "Salsa", null, null);

        Assert.Null(instructor.Bio);
        Assert.Null(instructor.PhotoUrl);
    }

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesFields()
    {
        var instructor = new Instructor("Laura Gómez", "Salsa", null, null);

        instructor.UpdateDetails("Laura G.", "Bachata", "Nueva bio", "https://example.com/nueva.jpg");

        Assert.Equal("Laura G.", instructor.FullName);
        Assert.Equal("Bachata", instructor.Specialty);
        Assert.Equal("Nueva bio", instructor.Bio);
        Assert.Equal("https://example.com/nueva.jpg", instructor.PhotoUrl);
    }

    [Fact]
    public void UpdateDetails_WithEmptyFullName_ThrowsArgumentException()
    {
        var instructor = new Instructor("Laura Gómez", "Salsa", null, null);

        Assert.Throws<ArgumentException>(() => instructor.UpdateDetails("", "Salsa", null, null));
    }

    [Fact]
    public void Deactivate_WhenCalled_SetsIsActiveToFalse()
    {
        var instructor = new Instructor("Laura Gómez", "Salsa", null, null);

        instructor.Deactivate();

        Assert.False(instructor.IsActive);
    }

    [Fact]
    public void Activate_WhenCalled_SetsIsActiveToTrue()
    {
        var instructor = new Instructor("Laura Gómez", "Salsa", null, null, isActive: false);

        instructor.Activate();

        Assert.True(instructor.IsActive);
    }
}
