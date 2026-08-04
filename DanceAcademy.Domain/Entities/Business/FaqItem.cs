#nullable enable
namespace DanceAcademy.Domain.Entities;

public sealed class FaqItem
{
    private FaqItem() { }

    public FaqItem(string question, string answer, string category, int order, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("La pregunta es obligatoria.", nameof(question));
        if (string.IsNullOrWhiteSpace(answer))
            throw new ArgumentException("La respuesta es obligatoria.", nameof(answer));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("La categoría es obligatoria.", nameof(category));
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "El orden debe ser >= 1.");

        Id = Guid.NewGuid();
        Question = question.Trim();
        Answer = answer.Trim();
        Category = category.Trim();
        Order = order;
        IsActive = isActive;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Question { get; private set; } = string.Empty;
    public string Answer { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void UpdateDetails(string question, string answer, string category, int order)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("La pregunta es obligatoria.", nameof(question));
        if (string.IsNullOrWhiteSpace(answer))
            throw new ArgumentException("La respuesta es obligatoria.", nameof(answer));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("La categoría es obligatoria.", nameof(category));
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "El orden debe ser >= 1.");

        Question = question.Trim();
        Answer = answer.Trim();
        Category = category.Trim();
        Order = order;
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
