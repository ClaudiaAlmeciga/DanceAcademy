using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Tests.Domain;

public class CertificateTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesCertificateWithVerificationCode()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var certificate = new Certificate(userId, courseId);

        Assert.Equal(userId, certificate.UserId);
        Assert.Equal(courseId, certificate.CourseId);
        Assert.False(string.IsNullOrWhiteSpace(certificate.VerificationCode));
        Assert.StartsWith("DA-", certificate.VerificationCode);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Certificate(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_WithEmptyCourseId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Certificate(Guid.NewGuid(), Guid.Empty));
    }

    [Fact]
    public void Constructor_WhenCalledTwice_GeneratesDifferentVerificationCodes()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var firstCertificate = new Certificate(userId, courseId);
        var secondCertificate = new Certificate(userId, courseId);

        Assert.NotEqual(firstCertificate.VerificationCode, secondCertificate.VerificationCode);
        Assert.NotEqual(firstCertificate.Id, secondCertificate.Id);
    }
}
