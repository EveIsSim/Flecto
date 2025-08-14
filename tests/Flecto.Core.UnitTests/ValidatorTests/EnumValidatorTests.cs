using Flecto.Core.Models.Filters;
using Flecto.Core.UnitTests.Collections;
using Flecto.Core.Validators;

namespace Flecto.Core.UnitTests.ValidatorTests;

[Collection(CollectionConsts.TestColletionName)]
public class EnumValidatorTests
{
    private enum BoxStatus
    {
        Opened,
        Destoyed,
        Closed
    };

    [Fact]
    public void Validate_NullFilter_AllowNullableTrue_ReturnsEmpty()
    {
        // Arrange
        EnumFilter<BoxStatus>? filter = null;

        // Act
        var result = EnumValidator.Validate(filter!, allowNullable: true);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_NullFilter_AllowNullableFalse_ReturnsError()
    {
        // Arrange
        EnumFilter<BoxStatus>? filter = null;

        // Act
        var result = EnumValidator.Validate(filter!, allowNullable: false);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("EnumFilter`1", error.Field);
        Assert.Equal("Filter must not be null", error.Error);
    }

    [Fact]
    public void Validate_EqAndNotEqSet_ReturnsError()
    {
        // Arrange
        var filter = new EnumFilter<BoxStatus>
        {
            Eq = BoxStatus.Opened,
            NotEq = BoxStatus.Closed
        };

        // Act
        var result = EnumValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("EnumFilter", error.Field);
        Assert.Equal("Cannot specify both Eq and NotEq simultaneously", error.Error);
    }

    [Fact]
    public void Validate_EmptyIn_ReturnsError()
    {
        // Arrange
        var filter = new EnumFilter<BoxStatus>
        {
            In = Array.Empty<BoxStatus>()
        };

        // Act
        var result = EnumValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("In", error.Field);
        Assert.Equal("Array cannot be empty if specified", error.Error);
    }

    [Fact]
    public void Validate_NotInWithDuplicates_ReturnsError()
    {
        // Arrange
        var filter = new EnumFilter<BoxStatus>
        {
            NotIn = new[] { BoxStatus.Opened, BoxStatus.Opened }
        };

        // Act
        var result = EnumValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("NotIn", error.Field);
        Assert.Equal("Array contains duplicate values", error.Error);
    }

    [Fact]
    public void Validate_CustomValidatorFails_ReturnsCustomError()
    {
        // Arrange
        var filter = new EnumFilter<BoxStatus> { Eq = BoxStatus.Opened };

        // Act
        var result = EnumValidator.Validate(
            filter,
            customValidator: _ => (false, "enum invalid"));

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("EnumFilter`1", error.Field);
        Assert.Equal("enum invalid", error.Error);
    }

    [Fact]
    public void Validate_ValidFilter_ReturnsEmpty()
    {
        // Arrange
        var filter = new EnumFilter<BoxStatus>
        {
            In = new[] { BoxStatus.Opened, BoxStatus.Destoyed },
            NotEq = BoxStatus.Closed
        };

        // Act
        var result = EnumValidator.Validate(filter);

        // Assert
        Assert.Empty(result);
    }

    #region EnsureValid

    [Fact]
    public void EnsureValid_WithValidFilter_DoesNotThrow()
    {
        // Arrange
        var filter = new EnumFilter<BoxStatus>
        {
            Eq = BoxStatus.Opened
        };

        // Act
        var ex = Record.Exception(() =>
            EnumValidator.EnsureValid(filter, "boxes", "status"));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void EnsureValid_WithInvalidFilter_ThrowsDetailedError()
    {
        // Arrange
        var filter = new EnumFilter<BoxStatus>
        {
            Eq = BoxStatus.Opened,
            NotEq = BoxStatus.Closed
        };

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            EnumValidator.EnsureValid(filter, "users", "status"));

        // Assert
        Assert.Contains("EnumFilter", ex.Message);
        Assert.Contains("table: 'users'", ex.Message);
        Assert.Contains("column: 'status'", ex.Message);
        Assert.Contains("Cannot specify both Eq and NotEq", ex.Message);
    }

    #endregion
}
