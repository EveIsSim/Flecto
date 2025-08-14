using Flecto.Core.Models.Filters;
using Flecto.Core.UnitTests.Collections;
using Flecto.Core.Validators;

namespace Flecto.Core.UnitTests.ValidatorTests;

[Collection(CollectionConsts.TestColletionName)]
public class PaginationValidatorTests
{
    [Fact]
    public void Validate_NullFilter_ReturnsError()
    {
        // Arrange
        PaginationFilter? filter = null;

        // Act
        var result = PaginationValidator.Validate(filter!);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("PaginationFilter", error.Field);
        Assert.Equal("PaginationFilter is required but was null", error.Error);
    }

    [Fact]
    public void Validate_LimitIsZero_ReturnsError()
    {
        // Arrange
        var filter = new PaginationFilter
        {
            Limit = 0,
            Page = 1
        };

        // Act
        var result = PaginationValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("Limit", error.Field);
        Assert.Equal("Value should be greater than 0", error.Error);
    }

    [Fact]
    public void Validate_PageIsNegative_ReturnsError()
    {
        // Arrange
        var filter = new PaginationFilter
        {
            Limit = 10,
            Page = -5
        };

        // Act
        var result = PaginationValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("Page", error.Field);
        Assert.Equal("Value should be greater than 0", error.Error);
    }

    [Fact]
    public void Validate_LimitExceedsMax_ReturnsError()
    {
        // Arrange
        var filter = new PaginationFilter
        {
            Limit = 101,
            Page = 1
        };

        // Act
        var result = PaginationValidator.Validate(filter, maxLimit: 100);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("Limit", error.Field);
        Assert.Equal("Limit cannot exceed 100", error.Error);
    }

    [Fact]
    public void Validate_ValidPagination_ReturnsEmpty()
    {
        // Arrange
        var filter = new PaginationFilter
        {
            Limit = 20,
            Page = 2
        };

        // Act
        var result = PaginationValidator.Validate(filter, maxLimit: 100);

        // Assert
        Assert.Empty(result);
    }

    #region EnsureValid

    [Fact]
    public void EnsureValid_WithValidFilter_DoesNotThrow()
    {
        // Arrange
        var filter = new PaginationFilter
        {
            Limit = 10,
            Page = 2
        };

        // Act
        var ex = Record.Exception(() => PaginationValidator.EnsureValid(filter, false));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void EnsureValid_WithInvalidFilter_ThrowsDetailedError()
    {
        // Arrange
        var filter = new PaginationFilter
        {
            Limit = 0,
            Page = -1
        };

        // Act
        var ex = Assert.Throws<ArgumentException>(() => PaginationValidator.EnsureValid(filter, false));

        // Assert
        var expected = """
            PaginationFilter: validation failed:
            Limit: Value should be greater than 0
            Page: Value should be greater than 0
            """;
        Assert.Equal(expected, ex.Message);
    }

    [Fact]
    public void EnsureValid_WithForbiddenPagination_ThrowsDetailedError()
    {
        // Arrange
        var filter = new PaginationFilter
        {
            Limit = 0,
            Page = -1
        };

        // Act
        var ex = Assert.Throws<ArgumentException>(() => PaginationValidator.EnsureValid(filter, true));

        // Assert
        var expected = """
            PaginationFilter: validation failed:
            Pagination (LIMIT/OFFSET) is not allowed when using COUNT(*) query.
            """;
        Assert.Equal(expected, ex.Message);
    }

    #endregion
}
