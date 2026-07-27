using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Tests.Domain;

public class UserTests
{
    [Fact]
    public void UpdateProfile_WithValidData_UpdatesFields()
    {
        var user = new User();
        var birthDate = new DateOnly(1995, 5, 20);

        user.UpdateProfile("Milena Almeciga", "3001234567", birthDate);

        Assert.Equal("Milena Almeciga", user.FullName);
        Assert.Equal("3001234567", user.Phone);
        Assert.Equal(birthDate, user.BirthDate);
    }

    [Fact]
    public void UpdateProfile_TrimsFullNameAndPhone()
    {
        var user = new User();

        user.UpdateProfile("  Milena  ", "  3001234567  ", null);

        Assert.Equal("Milena", user.FullName);
        Assert.Equal("3001234567", user.Phone);
    }

    [Fact]
    public void UpdateProfile_WithWhitespaceFullName_SetsFullNameToNull()
    {
        var user = new User();

        user.UpdateProfile("   ", null, null);

        Assert.Null(user.FullName);
    }

    [Fact]
    public void UpdateProfile_WithNullValues_ClearsExistingData()
    {
        var user = new User();
        user.UpdateProfile("Milena", "3001234567", new DateOnly(1995, 5, 20));

        user.UpdateProfile(null, null, null);

        Assert.Null(user.FullName);
        Assert.Null(user.Phone);
        Assert.Null(user.BirthDate);
    }
}
