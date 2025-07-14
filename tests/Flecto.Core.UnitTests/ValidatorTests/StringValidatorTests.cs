using Flecto.Core.Models.Filters;
using Flecto.Core.Validators;
using Flecto.Core.Validators.Enums;

namespace Flecto.Core.UnitTests.ValidatorTests;

public class StringValidatorTests
{
    [Fact]
    public void Validate_WithNullEq_DoesNotReturnErrors()
    {
        // Arrange
        var filter = new StringFilter { Eq = null };

        // Act
        var result = StringValidator.Validate(filter).ToArray();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WithEmptyString_WhenEmptyNotAllowed_ReturnsError()
    {
        // Arrange
        var filter = new StringFilter { Eq = "   " };

        // Act
        var result = StringValidator.Validate(filter, StringFilterValidationOptions.None).ToArray();

        // Assert
        var error = Assert.Single(result);
        Assert.Equal(nameof(filter.Eq), error.Field);
        Assert.Equal("Value cannot be empty", error.Error);
    }

    [Fact]
    public void Validate_WithMaxLengthExceeded_ReturnsError()
    {
        // Arrange
        var filter = new StringFilter { Eq = "1234567890" };

        // Act
        var result = StringValidator.Validate(filter, maxLength: 5).ToArray();

        // Assert
        var error = Assert.Single(result);
        Assert.Equal(nameof(filter.Eq), error.Field);
        Assert.Equal("Value exceeds max length of 5", error.Error);
    }

    [Fact]
    public void Validate_WithCustomValidatorFailure_ReturnsError()
    {
        // Arrange
        var filter = new StringFilter { StartsWith = "bad" };

        static (bool, string?) Custom(string input) => input == "ok"
            ? (true, null)
            : (false, "Only 'ok' is allowed");

        // Act
        var result = StringValidator.Validate(
            filter,
            options: StringFilterValidationOptions.All,
            customValidator: Custom).ToArray();

        // Assert
        var error = Assert.Single(result);
        Assert.Equal(nameof(filter.StartsWith), error.Field);
        Assert.Equal("Only 'ok' is allowed", error.Error);
    }

    [Fact]
    public void Validate_WithArrayCustomValidatorFailure_ReturnsError()
    {
        // Arrange
        var filter = new StringFilter { In = new[] { "a", "b", "fail" } };

        static (bool, string?) ArrayValidator(string[] arr)
            => arr.Contains("fail")
                ? (false, "Array contains forbidden value")
                : (true, null);

        // Act
        var result = StringValidator.Validate(
            filter,
            customArrayValidator: ArrayValidator).ToArray();

        // Assert
        var error = Assert.Single(result);
        Assert.Equal(nameof(filter.In), error.Field);
        Assert.Equal("Array contains forbidden value", error.Error);
    }

    #region EnsureValid

    [Fact]
    public void EnsureValid_WithValidFilter_DoesNotThrow()
    {
        // Arrange
        var filter = new StringFilter { Eq = "hello" };
        var table = "users";
        var column = "name";

        // Act
        var ex = Record.Exception(() =>
            StringValidator.EnsureValid(filter, table, column));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void EnsureValid_WithInvalidFilter_ThrowsArgumentException()
    {
        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            StringValidator.EnsureValid(null!, "users", "name"));

        // Assert
        Assert.Equal("""
            StringFilter: validation for table: 'users', column: 'name' failed:
            StringFilter: Filter must not be null
            """,
            ex.Message);
    }

    #endregion
}
