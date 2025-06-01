using Shared.Services;
using Xunit;

public class CalculatorTests
{
    [Fact]
    public void Add_ShouldReturnCorrectSum()
    {
        var calc = new Calculator();
        Assert.Equal(5, calc.Add(2, 3)); // Korrekt
    }

    [Fact]
    public void Subtract_ShouldReturnCorrectDifference()
    {
        var calc = new Calculator();
        Assert.Equal(1, calc.Subtract(3, 2)); // Korrekt
    }

    [Fact]
    public void Multiply_ShouldReturnCorrectProduct()
    {
        var calc = new Calculator();
        Assert.Equal(6, calc.Multiply(2, 3)); // Korrekt
    }

    [Fact]
    public void Divide_ShouldReturnCorrectQuotient()
    {
        var calc = new Calculator();
        Assert.Equal(2, calc.Divide(6, 3)); // Korrekt
    }

}
