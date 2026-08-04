namespace DanceAcademy.Public.Shared;

/// <summary>
/// Un eslabón de la navegación de migas de pan. <c>Url</c> null significa la página
/// actual (no es un enlace, se muestra como texto activo).
/// </summary>
public sealed record BreadcrumbItem(string Text, string? Url);
