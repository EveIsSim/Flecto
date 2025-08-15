using Flecto.Core.Models.Filters;
using Flecto.Core.UnitTests.Collections;
using Flecto.Core.Validators;

namespace Flecto.Core.UnitTests.ValidatorTests;

[Collection(CollectionConsts.TestColletionName)]
public class GuidValidatorTests
{
    [Fact]
    public void Validate_NullFilter_AllowNullableTrue_ReturnsEmpty()
    {
        // Arrange
        GuidFilter? filter = null;

        // Act
        var result = GuidValidator.Validate(filter!, allowNullable: true);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_NullFilter_AllowNullableFalse_ReturnsError()
    {
        // Arrange
        GuidFilter? filter = null;

        // Act
        var result = GuidValidator.Validate(filter!, allowNullable: false);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("GuidFilter", error.Field);
        Assert.Equal("Filter must not be null", error.Error);
    }

    [Fact]
    public void Validate_EqAndNotEqSet_ReturnsError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var filter = new GuidFilter
        {
            Eq = id,
            NotEq = id
        };

        // Act
        var error = Assert.Single(GuidValidator.Validate(filter));

        // Assert
        Assert.Equal("GuidFilter", error.Field);
        Assert.Equal("Cannot specify both Eq and NotEq simultaneously", error.Error);
    }

    [Fact]
    public void Validate_EmptyInArray_ReturnsError()
    {
        // Arrange
        var filter = new GuidFilter
        {
            In = []
        };

        // Act
        var result = GuidValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("In", error.Field);
        Assert.Equal("Array cannot be empty if specified", error.Error);
    }

    [Fact]
    public void Validate_NotInWithDuplicates_ReturnsError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var filter = new GuidFilter
        {
            NotIn = [id, id]
        };

        // Act
        var result = GuidValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("NotIn", error.Field);
        Assert.Equal("Array contains duplicate values", error.Error);
    }

    [Fact]
    public void Validate_CustomValidatorFails_ReturnsCustomError()
    {
        // Arrange
        var filter = new GuidFilter { Eq = Guid.NewGuid() };

        // Act
        var result = GuidValidator.Validate(filter, customValidator: static _ => (false, "custom error"));

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("GuidFilter", error.Field);
        Assert.Equal("custom error", error.Error);
    }

    [Fact]
    public void Validate_ValidFilter_ReturnsEmpty()
    {
        // Arrange
        var filter = new GuidFilter
        {
            In = [Guid.NewGuid(), Guid.NewGuid()],
            NotEq = Guid.NewGuid()
        };

        // Act
        var result = GuidValidator.Validate(filter);

        // Assert
        Assert.Empty(result);
    }

    #region EnsureValid

    [Fact]
    public void EnsureValid_WithValidFilter_DoesNotThrow()
    {
        // Arrange
        var filter = new GuidFilter
        {
            Eq = Guid.NewGuid()
        };

        // Act
        var ex = Record.Exception(() =>
            GuidValidator.EnsureValid(filter, "users", "id"));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void EnsureValid_WithEqAndNotEq_ThrowsValidationError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var filter = new GuidFilter
        {
            Eq = id,
            NotEq = id
        };

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            GuidValidator.EnsureValid(filter, "users", "id"));

        // Assert
        Assert.Contains("GuidFilter", ex.Message);
        Assert.Contains("table: 'users'", ex.Message);
        Assert.Contains("column: 'id'", ex.Message);
        Assert.Contains("Cannot specify both Eq and NotEq", ex.Message);
    }

    #endregion
}
