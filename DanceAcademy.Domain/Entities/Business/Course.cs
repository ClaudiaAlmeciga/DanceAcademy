#nullable enable
namespace DanceAcademy.Domain.Entities;

public sealed class Course
{
    // EF Core requiere constructor sin parámetros 
    private Course() { }

    public Course(
        string title,
        Guid levelId,
        string? description = null,
        bool isPublished = false,
        string? imageUrl = null,
        int? durationHours = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del curso es obligatorio.", nameof(title));
        if (levelId == Guid.Empty)
            throw new ArgumentException("LevelId es obligatorio.", nameof(levelId));
        if (durationHours is <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationHours), "La duración debe ser mayor a 0.");

        Id = Guid.NewGuid();
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        LevelId = levelId;
        IsPublished = isPublished;
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        DurationHours = durationHours;
        PricingType = PricingType.Free;
        Price = null;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid LevelId { get; private set; }
    public bool IsPublished { get; private set; }
    public string? ImageUrl { get; private set; }
    public int? DurationHours { get; private set; }
    public PricingType PricingType { get; private set; } = PricingType.Free;
    public decimal? Price { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Navegación
    private readonly List<Module> _modules = new();
    public IReadOnlyCollection<Module> Modules => _modules;

    private readonly List<SubscriptionPlan> _subscriptionPlans = new();
    public IReadOnlyCollection<SubscriptionPlan> SubscriptionPlans => _subscriptionPlans;

    public void Publish()
    {
        IsPublished = true;
        Touch();
    }

    public void Unpublish()
    {
        IsPublished = false;
        Touch();
    }

    public void SetLevel(Guid levelId)
    {
        if (levelId == Guid.Empty)
            throw new ArgumentException("LevelId es obligatorio.", nameof(levelId));

        LevelId = levelId;
        Touch();
    }

    public void SetPricing(PricingType pricingType, decimal? price)
    {
        var requiresPrice = pricingType is PricingType.IndividualPurchase or PricingType.Both;

        if (requiresPrice && (price is null || price <= 0))
            throw new ArgumentException("Este tipo de precio requiere un Price mayor a 0.", nameof(price));

        if (!requiresPrice && price is not null)
            throw new ArgumentException("Este tipo de precio no admite un Price (debe ser null).", nameof(price));

        PricingType = pricingType;
        Price = price;
        Touch();
    }

    // Llamar después de SetPricing — depende de que PricingType ya esté actualizado.
    public void SetSubscriptionPlans(IEnumerable<SubscriptionPlan> plans)
    {
        var planList = plans.ToList();
        var allowsPlans = PricingType is PricingType.SubscriptionIncluded or PricingType.Both;

        if (!allowsPlans && planList.Count > 0)
            throw new ArgumentException("Este tipo de precio no admite planes de suscripción asociados.", nameof(plans));

        _subscriptionPlans.Clear();
        _subscriptionPlans.AddRange(planList);
        Touch();
    }

    public void UpdateDetails(string title, string? description, string? imageUrl, int? durationHours)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del curso es obligatorio.", nameof(title));
        if (durationHours is <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationHours), "La duración debe ser mayor a 0.");

        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        DurationHours = durationHours;
        Touch();
    }

    public Module AddModule(string title, int order)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del módulo es obligatorio.", nameof(title));
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "El orden debe ser >= 1.");

        var module = new Module(courseId: Id, title: title.Trim(), order: order);
        _modules.Add(module);
        Touch();
        return module;
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

}
