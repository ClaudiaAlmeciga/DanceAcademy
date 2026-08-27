using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Tests.Domain;

public class EventRegistrationTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesPendingRegistration()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var registration = new EventRegistration(userId, eventId);

        Assert.Equal(userId, registration.UserId);
        Assert.Equal(eventId, registration.EventId);
        Assert.Equal(EventRegistrationStatus.Pending, registration.Status);
        Assert.Null(registration.PaidAt);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new EventRegistration(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_WithEmptyEventId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new EventRegistration(Guid.NewGuid(), Guid.Empty));
    }

    [Fact]
    public void MarkPaid_WhenCalled_SetsStatusToPaidAndPaidAt()
    {
        var registration = new EventRegistration(Guid.NewGuid(), Guid.NewGuid());

        registration.MarkPaid();

        Assert.Equal(EventRegistrationStatus.Paid, registration.Status);
        Assert.NotNull(registration.PaidAt);
    }
}
