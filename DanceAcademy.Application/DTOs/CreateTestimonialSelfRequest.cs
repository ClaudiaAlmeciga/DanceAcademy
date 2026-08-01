#nullable enable
namespace DanceAcademy.Application.DTOs;

/// <summary>
/// Envío de un testimonio por el propio estudiante autenticado. No incluye StudentName
/// ni IsPublished — el nombre se toma del perfil del usuario y todo envío arranca sin
/// publicar, pendiente de moderación (ver <c>MeTestimonialsEndpoints</c>).
/// </summary>
public sealed record CreateTestimonialSelfRequest(
    string Content,
    int Rating,
    Guid? CourseId
);

public sealed record MyTestimonialDto(
    Guid Id,
    string Content,
    int Rating,
    Guid? CourseId,
    string? CourseTitle,
    bool IsPublished,
    DateTimeOffset CreatedAt
);
