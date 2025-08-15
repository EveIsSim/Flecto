using Flecto.Core.Models.Filters;
using Flecto.Core.UnitTests.Collections;
using Flecto.Core.Validators;

namespace Flecto.Core.UnitTests.ValidatorTests;

[Collection(CollectionConsts.TestColletionName)]
public class NumericValidatorTests
{
    [Fact]
    public void Validate_NullFilter_AllowNullableTrue_ReturnsEmpty()
    {
        // Arrange
        NumericFilter<int>? filter = null;

        // Act
        var result = NumericValidator.Validate(filter!, allowNullable: true);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_NullFilter_AllowNullableFalse_ReturnsError()
    {
        // Arrange
        NumericFilter<int>? filter = null;

        // Act
        var result = NumericValidator.Validate(filter!, allowNullable: false);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("NumericFilter`1", error.Field);
        Assert.Equal("Filter must not be null", error.Error);
    }

    [Fact]
    public void Validate_EqAndNotEqSet_ReturnsError()
    {
        // Arrange
        var filter = new NumericFilter<int>
        {
            Eq = 5,
            NotEq = 5
        };

        // Act
        var result = NumericValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("NumericFilter", error.Field);
        Assert.Equal("Cannot specify both Eq and NotEq simultaneously", error.Error);
    }

    [Fact]
    public void Validate_InvalidRange_GtGreaterThanLt_ReturnsError()
    {
        // Arrange
        var filter = new NumericFilter<int>
        {
            Gt = 10,
            Lt = 5
        };

        // Act
        var result = NumericValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("NumericFilter", error.Field);
        Assert.Equal("Gt (10) must be less than Lt (5)", error.Error);
    }

    [Fact]
    public void Validate_EmptyIn_ReturnsError()
    {
        // Arrange
        var filter = new NumericFilter<int>
        {
            In = []
        };

        // Act
        var result = NumericValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("In", error.Field);
        Assert.Equal("Array cannot be empty if specified", error.Error);
    }

    [Fact]
    public void Validate_NotInWithDuplicates_ReturnsError()
    {
        // Arrange
        var filter = new NumericFilter<int>
        {
            NotIn = [2, 2]
        };

        // Act
        var result = NumericValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("NotIn", error.Field);
        Assert.Equal("Array contains duplicate values", error.Error);
    }

    [Fact]
    public void Validate_CustomValidatorFails_ReturnsCustomError()
    {
        // Arrange
        var filter = new NumericFilter<int> { Eq = 42 };

        // Act
        var result = NumericValidator.Validate(
                filter, customValidator: static _ => (false, "custom check failed"));

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("NumericFilter`1", error.Field);
        Assert.Equal("custom check failed", error.Error);
    }

    [Fact]
    public void Validate_AllCorrect_ReturnsEmpty()
    {
        // Arrange
        var filter = new NumericFilter<int>
        {
            Gte = 10,
            Lte = 20,
            In = [10, 15],
            NotIn = [0]
        };

        // Act
        var result = NumericValidator.Validate(filter);

        // Assert
        Assert.Empty(result);
    }

    #region EnsureValid

    [Fact]
    public void EnsureValid_ValidFilter_DoesNotThrow()
    {
        // Arrange
        var filter = new NumericFilter<int>
        {
            Gte = 1,
            Lte = 10
        };

        // Act
        var ex = Record.Exception(() =>
            NumericValidator.EnsureValid(filter, "products", "price"));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void EnsureValid_InvalidRange_ThrowsError()
    {
        // Arrange
        var filter = new NumericFilter<int>
        {
            Gt = 20,
            Lt = 10
        };

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            NumericValidator.EnsureValid(filter, "products", "price"));

        // Assert
        Assert.Contains("NumericFilter", ex.Message);
        Assert.Contains("table: 'products'", ex.Message);
        Assert.Contains("column: 'price'", ex.Message);
        Assert.Contains("Gt (20) must be less than Lt (10)", ex.Message);
    }

    #endregion
}
