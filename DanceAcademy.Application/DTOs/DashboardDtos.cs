#nullable enable
namespace DanceAcademy.Application.DTOs;

/// <summary>
/// Resumen de avance del usuario autenticado en un curso en el que está inscrito.
/// </summary>
public sealed record MyCourseProgressSummaryDto(
    Guid CourseId,
    string CourseTitle,
    int TotalLessons,
    int CompletedLessons,
    double ProgressPercentage
);

/// <summary>
/// Panel del estudiante: cuántos cursos tiene y su avance general.
/// </summary>
public sealed record MyDashboardDto(
    int EnrolledCoursesCount,
    double OverallProgressPercentage,
    IReadOnlyList<MyCourseProgressSummaryDto> Courses
);
