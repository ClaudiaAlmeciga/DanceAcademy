using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Application.DTOs;

public sealed record CreateEventRegistrationRequest(Guid EventId);

public sealed record MyEventRegistrationDto(
    Guid Id,
    Guid EventId,
    string EventTitle,
    DateTimeOffset EventDate,
    string? Location,
    decimal Price,
    EventRegistrationStatus Status,
    DateTimeOffset RegisteredAt,
    string? ImageUrl
);
