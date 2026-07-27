using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Tests.Domain;

public class SubscriptionPlanTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesActivePlan()
    {
        var plan = new SubscriptionPlan("Básico", "Acceso a cursos básicos", 19900m, 30);

        Assert.True(plan.IsActive);
        Assert.Equal(19900m, plan.Price);
        Assert.Equal(30, plan.BillingPeriodDays);
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SubscriptionPlan("", null, 19900m, 30));
    }

    [Fact]
    public void Constructor_WithZeroPrice_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SubscriptionPlan("Básico", null, 0m, 30));
    }

    [Fact]
    public void Constructor_WithNegativePrice_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SubscriptionPlan("Básico", null, -100m, 30));
    }

    [Fact]
    public void Constructor_WithBillingPeriodLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SubscriptionPlan("Básico", null, 19900m, 0));
    }

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesFields()
    {
        var plan = new SubscriptionPlan("Básico", null, 19900m, 30);

        plan.UpdateDetails("Premium", "Acceso total", 39900m, 365);

        Assert.Equal("Premium", plan.Name);
        Assert.Equal(39900m, plan.Price);
        Assert.Equal(365, plan.BillingPeriodDays);
    }

    [Fact]
    public void Deactivate_WhenCalled_SetsIsActiveToFalse()
    {
        var plan = new SubscriptionPlan("Básico", null, 19900m, 30);

        plan.Deactivate();

        Assert.False(plan.IsActive);
    }
}
