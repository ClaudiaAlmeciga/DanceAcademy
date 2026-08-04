using DanceAcademy.Domain.Entities;

namespace DanceAcademy.Tests.Domain;

public class FaqItemTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesActiveFaqItem()
    {
        var faqItem = new FaqItem("¿Cómo me inscribo?", "Desde el catálogo de cursos.", "Cuenta", 1);

        Assert.True(faqItem.IsActive);
        Assert.Equal("¿Cómo me inscribo?", faqItem.Question);
        Assert.Equal("Desde el catálogo de cursos.", faqItem.Answer);
        Assert.Equal("Cuenta", faqItem.Category);
        Assert.Equal(1, faqItem.Order);
    }

    [Fact]
    public void Constructor_WithEmptyQuestion_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new FaqItem("", "Respuesta", "Cuenta", 1));
    }

    [Fact]
    public void Constructor_WithEmptyAnswer_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new FaqItem("¿Pregunta?", "", "Cuenta", 1));
    }

    [Fact]
    public void Constructor_WithEmptyCategory_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new FaqItem("¿Pregunta?", "Respuesta", "", 1));
    }

    [Fact]
    public void Constructor_WithOrderLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FaqItem("¿Pregunta?", "Respuesta", "Cuenta", 0));
    }

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesFields()
    {
        var faqItem = new FaqItem("¿Pregunta?", "Respuesta", "Cuenta", 1);

        faqItem.UpdateDetails("¿Pregunta nueva?", "Respuesta nueva", "Pagos", 2);

        Assert.Equal("¿Pregunta nueva?", faqItem.Question);
        Assert.Equal("Respuesta nueva", faqItem.Answer);
        Assert.Equal("Pagos", faqItem.Category);
        Assert.Equal(2, faqItem.Order);
    }

    [Fact]
    public void UpdateDetails_WithOrderLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        var faqItem = new FaqItem("¿Pregunta?", "Respuesta", "Cuenta", 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => faqItem.UpdateDetails("¿Pregunta?", "Respuesta", "Cuenta", 0));
    }

    [Fact]
    public void Deactivate_WhenCalled_SetsIsActiveToFalse()
    {
        var faqItem = new FaqItem("¿Pregunta?", "Respuesta", "Cuenta", 1);

        faqItem.Deactivate();

        Assert.False(faqItem.IsActive);
    }

    [Fact]
    public void Activate_WhenCalled_SetsIsActiveToTrue()
    {
        var faqItem = new FaqItem("¿Pregunta?", "Respuesta", "Cuenta", 1, isActive: false);

        faqItem.Activate();

        Assert.True(faqItem.IsActive);
    }
}
