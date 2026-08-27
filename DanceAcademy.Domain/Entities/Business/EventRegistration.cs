#nullable enable
namespace DanceAcademy.Domain.Entities;

/// <summary>
/// Representa la inscripción de un usuario a un evento. Los eventos tienen precio propio,
/// independiente del de los cursos; mientras la pasarela de pago no esté conectada, la
/// inscripción queda en <see cref="EventRegistrationStatus.Pending"/> y el Admin la marca
/// como pagada manualmente (ver <c>AdminEventsEndpoints</c>).
/// </summary>
public sealed class EventRegistration
{
    // EF Core requiere constructor sin parámetros
    private EventRegistration() { }

    public EventRegistration(Guid userId, Guid eventId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId es obligatorio.", nameof(userId));
        if (eventId == Guid.Empty)
            throw new ArgumentException("EventId es obligatorio.", nameof(eventId));

        Id = Guid.NewGuid();
        UserId = userId;
        EventId = eventId;
        Status = EventRegistrationStatus.Pending;
        RegisteredAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid EventId { get; private set; }
    public EventRegistrationStatus Status { get; private set; }
    public DateTimeOffset RegisteredAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }

    public void MarkPaid()
    {
        Status = EventRegistrationStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;
    }
}
