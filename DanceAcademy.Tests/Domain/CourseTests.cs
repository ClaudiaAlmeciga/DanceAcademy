using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Tests.Domain;

public class CourseTests
{
    private static Guid ValidLevelId => Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_CreatesUnpublishedFreeCourse()
    {
        var course = new Course("Salsa Principiantes", ValidLevelId, "Descripción");

        Assert.False(course.IsPublished);
        Assert.Equal(PricingType.Free, course.PricingType);
        Assert.Null(course.Price);
    }

    [Fact]
    public void Constructor_WithEmptyTitle_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Course("", ValidLevelId));
    }

    [Fact]
    public void Constructor_WithWhitespaceTitle_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Course("   ", ValidLevelId));
    }

    [Fact]
    public void Constructor_WithEmptyLevelId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Course("Salsa", Guid.Empty));
    }

    [Fact]
    public void Constructor_TrimsTitleAndDescription()
    {
        var course = new Course("  Salsa  ", ValidLevelId, "  Descripción  ");

        Assert.Equal("Salsa", course.Title);
        Assert.Equal("Descripción", course.Description);
    }

    [Fact]
    public void Constructor_WithWhitespaceDescription_SetsDescriptionToNull()
    {
        var course = new Course("Salsa", ValidLevelId, "   ");

        Assert.Null(course.Description);
    }

    [Fact]
    public void Publish_WhenCalled_SetsIsPublishedToTrue()
    {
        var course = new Course("Salsa", ValidLevelId);

        course.Publish();

        Assert.True(course.IsPublished);
    }

    [Fact]
    public void Unpublish_WhenCalled_SetsIsPublishedToFalse()
    {
        var course = new Course("Salsa", ValidLevelId, isPublished: true);

        course.Unpublish();

        Assert.False(course.IsPublished);
    }

    [Fact]
    public void SetLevel_WithValidId_UpdatesLevelId()
    {
        var course = new Course("Salsa", ValidLevelId);
        var newLevelId = Guid.NewGuid();

        course.SetLevel(newLevelId);

        Assert.Equal(newLevelId, course.LevelId);
    }

    [Fact]
    public void SetLevel_WithEmptyId_ThrowsArgumentException()
    {
        var course = new Course("Salsa", ValidLevelId);

        Assert.Throws<ArgumentException>(() => course.SetLevel(Guid.Empty));
    }

    [Fact]
    public void SetPricing_IndividualPurchaseWithoutPrice_ThrowsArgumentException()
    {
        var course = new Course("Salsa", ValidLevelId);

        Assert.Throws<ArgumentException>(() => course.SetPricing(PricingType.IndividualPurchase, null));
    }

    [Fact]
    public void SetPricing_IndividualPurchaseWithZeroPrice_ThrowsArgumentException()
    {
        var course = new Course("Salsa", ValidLevelId);

        Assert.Throws<ArgumentException>(() => course.SetPricing(PricingType.IndividualPurchase, 0m));
    }

    [Fact]
    public void SetPricing_FreeWithPrice_ThrowsArgumentException()
    {
        var course = new Course("Salsa", ValidLevelId);

        Assert.Throws<ArgumentException>(() => course.SetPricing(PricingType.Free, 10000m));
    }

    [Fact]
    public void SetPricing_IndividualPurchaseWithValidPrice_UpdatesPricingTypeAndPrice()
    {
        var course = new Course("Salsa", ValidLevelId);

        course.SetPricing(PricingType.IndividualPurchase, 60000m);

        Assert.Equal(PricingType.IndividualPurchase, course.PricingType);
        Assert.Equal(60000m, course.Price);
    }

    [Fact]
    public void SetSubscriptionPlans_WhenPricingTypeIsFree_ThrowsArgumentException()
    {
        var course = new Course("Salsa", ValidLevelId);
        var plan = new SubscriptionPlan("Básico", null, 19900m, 30);

        Assert.Throws<ArgumentException>(() => course.SetSubscriptionPlans([plan]));
    }

    [Fact]
    public void SetSubscriptionPlans_WhenPricingTypeAllowsPlans_AssignsPlans()
    {
        var course = new Course("Salsa", ValidLevelId);
        course.SetPricing(PricingType.SubscriptionIncluded, null);
        var plan = new SubscriptionPlan("Básico", null, 19900m, 30);

        course.SetSubscriptionPlans([plan]);

        Assert.Single(course.SubscriptionPlans);
    }

    [Fact]
    public void UpdateDetails_WithEmptyTitle_ThrowsArgumentException()
    {
        var course = new Course("Salsa", ValidLevelId);

        Assert.Throws<ArgumentException>(() => course.UpdateDetails("", "desc"));
    }

    [Fact]
    public void AddModule_WithValidData_AddsModuleToCollection()
    {
        var course = new Course("Salsa", ValidLevelId);

        var module = course.AddModule("Módulo 1", 1);

        Assert.Single(course.Modules);
        Assert.Equal(course.Id, module.CourseId);
    }

    [Fact]
    public void AddModule_WithInvalidOrder_ThrowsArgumentOutOfRangeException()
    {
        var course = new Course("Salsa", ValidLevelId);

        Assert.Throws<ArgumentOutOfRangeException>(() => course.AddModule("Módulo 1", 0));
    }
}
