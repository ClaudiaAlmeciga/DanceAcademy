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

/// <summary>
/// Indica si el usuario autenticado cumple la recomendación de continuidad de niveles
/// (haber completado al 100% un curso del nivel inmediatamente anterior) antes de ver
/// o inscribirse en un curso de un nivel superior. <c>PreviousLevelName</c> es null
/// cuando el curso ya es del nivel más bajo (no hay nivel anterior que exigir).
/// </summary>
public sealed record LevelReadinessDto(
    bool MeetsRecommendation,
    string? PreviousLevelName
);
