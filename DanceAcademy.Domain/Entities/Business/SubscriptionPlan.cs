#nullable enable
namespace DanceAcademy.Domain.Entities;

public sealed class SubscriptionPlan
{
    private SubscriptionPlan() { }

    public SubscriptionPlan(string name, string? description, decimal price, int billingPeriodDays, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del plan es obligatorio.", nameof(name));
        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), "El precio debe ser mayor a 0.");
        if (billingPeriodDays < 1)
            throw new ArgumentOutOfRangeException(nameof(billingPeriodDays), "El período de facturación debe ser >= 1 día.");

        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Price = price;
        BillingPeriodDays = billingPeriodDays;
        IsActive = isActive;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int BillingPeriodDays { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void UpdateDetails(string name, string? description, decimal price, int billingPeriodDays)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del plan es obligatorio.", nameof(name));
        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), "El precio debe ser mayor a 0.");
        if (billingPeriodDays < 1)
            throw new ArgumentOutOfRangeException(nameof(billingPeriodDays), "El período de facturación debe ser >= 1 día.");

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Price = price;
        BillingPeriodDays = billingPeriodDays;
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
