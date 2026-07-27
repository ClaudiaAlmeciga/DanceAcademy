#nullable enable
namespace DanceAcademy.Application.DTOs;

/// <summary>
/// Estado de avance de una lección puntual para el usuario autenticado.
/// </summary>
public sealed record LessonProgressDto(
    Guid LessonId,
    bool IsCompleted,
    DateTimeOffset? CompletedAt
);

/// <summary>
/// Porcentaje de avance del usuario autenticado en un curso, calculado sobre
/// las lecciones publicadas (lección + módulo + curso publicados).
/// </summary>
public sealed record CourseProgressDto(
    Guid CourseId,
    string CourseTitle,
    int TotalLessons,
    int CompletedLessons,
    double ProgressPercentage
);

/// <summary>
/// Lección del curso (ordenada) con su estado de avance, para el menú de
/// navegación entre lecciones y para calcular la "siguiente lección".
/// </summary>
public sealed record LessonProgressListItemDto(
    Guid LessonId,
    Guid ModuleId,
    string ModuleTitle,
    string LessonTitle,
    int Order,
    bool IsCompleted
);
