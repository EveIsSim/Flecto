using Flecto.Core.Models.Filters;
using Flecto.Core.UnitTests.Collections;
using Flecto.Core.Validators;

namespace Flecto.Core.UnitTests.ValidatorTests;

[Collection(CollectionConsts.TestColletionName)]
public class DateValidatorTests
{
    [Fact]
    public void Validate_NullFilter_AllowNullableTrue_ReturnsEmpty()
    {
        // Arrange
        DateFilter? filter = null;

        // Act
        var result = DateValidator.Validate(filter!, allowNullable: true);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_NullFilter_AllowNullableFalse_ReturnsError()
    {
        // Arrange
        DateFilter? filter = null;

        // Act
        var result = DateValidator.Validate(filter!, allowNullable: false);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("DateFilter", error.Field);
        Assert.Equal("Filter must not be null", error.Error);
    }

    [Fact]
    public void Validate_EqAndNotEq_Set_ReturnsError()
    {
        // Arrange
        var filter = new DateFilter
        {
            Eq = DateTime.Today,
            NotEq = DateTime.Today
        };

        // Act
        var result = DateValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("DateFilter", error.Field);
        Assert.Contains("Cannot specify both Eq and NotEq", error.Error);
    }

    [Fact]
    public void Validate_InvalidRange_GtGreaterThanLt_ReturnsError()
    {
        // Arrange
        var filter = new DateFilter
        {
            Gt = new DateTime(2024, 12, 31, 00, 00, 00),
            Lt = new DateTime(2024, 1, 1, 00, 00, 00)
        };

        // Act
        var result = DateValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("DateFilter", error.Field);
        Assert.Equal(
            "Gt (12/31/2024 12:00:00 AM) must be less than Lt (1/1/2024 12:00:00 AM)",
            error.Error);
    }

    [Fact]
    public void Validate_EmptyInArray_ReturnsError()
    {
        // Arrange
        var filter = new DateFilter
        {
            In = []
        };

        // Act
        var result = DateValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("In", error.Field);
        Assert.Equal("Array cannot be empty if specified", error.Error);
    }

    [Fact]
    public void Validate_NotInArrayWithDuplicates_ReturnsError()
    {
        // Arrange
        var value = new DateTime(2024, 5, 1);
        var filter = new DateFilter
        {
            NotIn = [value, value]
        };

        // Act
        var result = DateValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("NotIn", error.Field);
        Assert.Equal("Array contains duplicate values", error.Error);
    }

    [Fact]
    public void Validate_CustomValidatorFails_ReturnsCustomError()
    {
        // Arrange
        var filter = new DateFilter { Eq = new DateTime(2024, 1, 1) };

        // Act
        var result = DateValidator.Validate(
            filter,
            customValidator: static f => (false, "Date is invalid"));

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("DateFilter", error.Field);
        Assert.Equal("Date is invalid", error.Error);
    }

    [Fact]
    public void Validate_AllCorrect_ReturnsEmpty()
    {
        // Arrange
        var filter = new DateFilter
        {
            Gte = new DateTime(2024, 1, 1),
            Lte = new DateTime(2024, 12, 31),
            In = [new DateTime(2024, 5, 1), new DateTime(2024, 6, 1)],
            NotIn = [new DateTime(2024, 7, 1)]
        };

        // Act
        var result = DateValidator.Validate(filter);

        // Assert
        Assert.Empty(result);
    }

    #region EnsureValid

    [Fact]
    public void EnsureValid_WithValidFilter_DoesNotThrow()
    {
        // Arrange
        var filter = new DateFilter
        {
            Gt = new DateTime(2024, 1, 1),
            Lt = new DateTime(2024, 12, 31)
        };

        // Act
        var ex = Record.Exception(() =>
            DateValidator.EnsureValid(filter, "events", "created_at"));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void EnsureValid_WithInvalidFilter_ThrowsWithDetails()
    {
        // Arrange
        var filter = new DateFilter
        {
            Gt = new DateTime(2024, 12, 31),
            Lt = new DateTime(2024, 1, 1)
        };

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            DateValidator.EnsureValid(filter, "events", "created_at"));

        // Assert
        Assert.Contains("DateFilter: validation for table: 'events', column: 'created_at' failed:", ex.Message);
        Assert.Contains("Gt (12/31/2024", ex.Message);
        Assert.Contains("must be less than Lt", ex.Message);
    }

    #endregion
}
