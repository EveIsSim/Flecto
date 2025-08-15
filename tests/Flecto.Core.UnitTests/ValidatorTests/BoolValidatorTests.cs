using Flecto.Core.Models.Filters;
using Flecto.Core.UnitTests.Collections;
using Flecto.Core.Validators;
using Flecto.Core.Validators.Enums;

namespace Flecto.Core.UnitTests.ValidatorTests;

[Collection(CollectionConsts.TestColletionName)]
public class BoolValidatorTests
{
    [Fact]
    public void Validate_NullFilter_WithAllowNullableOption_ReturnsEmpty()
    {
        // Act
        var result = BoolValidator.Validate(null, BoolFilterValidationOptions.AllowNullable);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_NullFilter_WithoutAllowNullableOption_ReturnsError()
    {
        // Act
        var result = BoolValidator.Validate(null).ToArray();

        // Assert
        _ = Assert.Single(result);

        var error = result.First();
        Assert.Equal("BoolFilter", error.Field);
        Assert.Equal("Filter must not be null", error.Error);
    }

    [Fact]
    public void Validate_BothEqAndNotEqSpecified_ReturnsError()
    {
        // Arrange
        var filter = new BoolFilter
        {
            Eq = true,
            NotEq = false
        };

        // Act
        var result = BoolValidator.Validate(filter).ToArray();

        // Assert
        _ = Assert.Single(result);

        var error = result.First();
        Assert.Equal("BoolFilter", error.Field);
        Assert.Equal("Cannot specify both Eq and NotEq simultaneously", error.Error);
    }

    [Fact]
    public void Validate_RequireAtLeastOne_WithEmptyFilter_ReturnsError()
    {
        // Arrange
        var filter = new BoolFilter();

        // Act
        var result = BoolValidator
            .Validate(filter, BoolFilterValidationOptions.RequireAtLeastOne)
            .ToArray();

        // Assert
        var error = result.First();

        Assert.Equal("BoolFilter", error.Field);
        Assert.Equal("At least one of Eq, NotEq, Null must be specified", error.Error);
    }

    [Fact]
    public void Validate_WithEmptyFilter_NoValidationOptions_ReturnsNoErrors()
    {
        // Arrange
        var filter = new BoolFilter();

        // Act
        var result = BoolValidator
            .Validate(filter, BoolFilterValidationOptions.None)
            .ToArray();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ValidEqFilter_ReturnsEmpty()
    {
        // Arrange
        var filter = new BoolFilter
        {
            Eq = true
        };

        // Act
        var result = BoolValidator.Validate(filter);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithCustomValidatorError_ReturnsCustomError()
    {
        // Arrange
        var filter = new BoolFilter { Eq = true };

        // Act
        var result = BoolValidator.Validate(
            filter,
            static f => (false, "Custom error occurred"),
            BoolFilterValidationOptions.None);

        // Assert
        var error = result.Single();
        Assert.Equal("BoolFilter", error.Field);
        Assert.Equal("Custom error occurred", error.Error);
    }

    [Fact]
    public void EnsureValid_WithInvalidFilter_ThrowsException()
    {
        // Arrange
        var filter = new BoolFilter { Eq = true, NotEq = false };

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            BoolValidator.EnsureValid(filter, "my_table", "my_column"));

        // Assert
        Assert.Equal(
            """
            BoolFilter: validation for table: 'my_table', column: 'my_column' failed:
            BoolFilter: Cannot specify both Eq and NotEq simultaneously
            """,
            ex.Message);
    }

    [Fact]
    public void EnsureValid_WithValidFilter_DoesNotThrow()
    {
        // Arrange
        var filter = new BoolFilter { Eq = true };

        // Act
        var ex = Record.Exception(() =>
            BoolValidator.EnsureValid(filter, "my_table", "my_column"));

        // Assert
        Assert.Null(ex);
    }
}
