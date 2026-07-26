namespace DanceAcademy.Application.DTOs.Admin;

public sealed record CreateCourseRequest(string Title, string? Description, string Level = "Beginner");
public sealed record UpdateCourseRequest(string Title, string? Description, string Level);

public sealed record CreateModuleRequest(string Title, int Order);
public sealed record UpdateModuleRequest(string Title, int Order);

public sealed record CreateLessonRequest(string Title, int Order, string? Content, string? VideoUrl);
public sealed record UpdateLessonRequest(string Title, int Order, string? Content, string? VideoUrl);
