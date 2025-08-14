using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;
using Flecto.Core.UnitTests.Collections;
using Flecto.Core.Validators;

namespace Flecto.Core.UnitTests.ValidatorTests;

[Collection(CollectionConsts.TestColletionName)]
public class SearchValidatorTests
{
    [Fact]
    public void Validate_NullFilter_AllowNullableTrue_ReturnsEmpty()
    {
        // Arrange
        SearchFilter? filter = null;

        // Act
        var result = SearchValidator.Validate(filter!, allowNullable: true);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_NullFilter_AllowNullableFalse_ReturnsError()
    {
        // Arrange
        SearchFilter? filter = null;

        // Act
        var result = SearchValidator.Validate(filter!, allowNullable: false);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("SearchFilter", error.Field);
        Assert.Equal("Filter must not be null", error.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyOrWhitespaceValue_ReturnsError(string? value)
    {
        // Arrange
        var filter = new SearchFilter { Value = value! };

        // Act
        var result = SearchValidator.Validate(filter, allowNullable: false);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("Value", error.Field);
        Assert.Contains("cannot be empty or whitespace", error.Error);
    }

    [Fact]
    public void Validate_ValueShorterThanMinLength_ReturnsError()
    {
        // Arrange
        var filter = new SearchFilter { Value = "hi" };

        // Act
        var result = SearchValidator.Validate(filter, minLength: 5);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("Value", error.Field);
        Assert.Equal("Value must be at least 5 characters", error.Error);
    }

    [Fact]
    public void Validate_ValueLongerThanMaxLength_ReturnsError()
    {
        // Arrange
        var filter = new SearchFilter { Value = "abcdefgh" };

        // Act
        var result = SearchValidator.Validate(filter, maxLength: 5);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("Value", error.Field);
        Assert.Equal("Value cannot exceed 5 characters", error.Error);
    }

    [Fact]
    public void Validate_ValueWithinLengthConstraints_ReturnsEmpty()
    {
        // Arrange
        var filter = new SearchFilter { Value = "hello" };

        // Act
        var result = SearchValidator.Validate(filter, minLength: 3, maxLength: 10);

        // Assert
        Assert.Empty(result);
    }

    #region EnsureValid

    [Fact]
    public void EnsureValid_ValidSearchFilter_DoesNotThrow()
    {
        // Arrange
        var filter = new SearchFilter { Value = "query" };
        var tablesWithColumns = new[] { ("posts", new[] { "title", "content" }) };

        // Act
        var ex = Record.Exception(() => SearchValidator.EnsureValid(filter, tablesWithColumns));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void EnsureValid_InvalidSearchFilter_ThrowsArgumentException()
    {
        // Arrange
        var filter = new SearchFilter { Value = " " };
        var tablesWithColumns = new[] { ("posts", new[] { "title" }) };

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            SearchValidator.EnsureValid(filter, tablesWithColumns));

        // Assert
        Assert.Contains("SearchFilter validation failed", ex.Message);
        Assert.Contains("Value is required", ex.Message);
    }

    #endregion

    #region EnsureValidTsVector

    [Fact]
    public void EnsureValidTsVector_PostgresDialect_DoesNotThrow()
    {
        // Arrange
        var filter = new SearchFilter { Value = "text" };
        var tablesWithColumns = new[] { ("docs", new[] { "title", "body" }) };

        // Act
        var ex = Record.Exception(() =>
            SearchValidator.EnsureValidTsVector(filter, tablesWithColumns, DialectType.Postgres));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void EnsureValidTsVector_NonPostgresDialect_ThrowsException()
    {
        // Arrange
        var filter = new SearchFilter { Value = "data" };
        var tablesWithColumns = new[] { ("any", new[] { "column" }) };

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            SearchValidator.EnsureValidTsVector(filter, tablesWithColumns, DialectType.Unknown));

        // Assert
        Assert.Equal("SearchTsvector is supported only for Postgres dialect.", ex.Message);
    }

    #endregion

}
