#nullable enable
namespace DanceAcademy.Application.DTOs.Public;

public sealed record CoursesQuery(int Page = 1, int PageSize = 12, Guid? LevelId = null)
{
    public (bool IsValid, string? Error) Validate()
    {
        if (Page < 1) return (false, "page debe ser >= 1.");
        if (PageSize < 1 || PageSize > 50) return (false, "pageSize debe estar entre 1 y 50.");
        return (true, null);
    }
}
