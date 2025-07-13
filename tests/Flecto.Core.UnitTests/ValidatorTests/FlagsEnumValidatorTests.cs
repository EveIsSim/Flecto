using Flecto.Core.Models.Filters;
using Flecto.Core.Validators;

namespace Flecto.Core.UnitTests.ValidatorTests;

public class FlagsEnumValidatorTests
{
    [Flags]
    private enum AccessRights
    {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 4
    }

    [Fact]
    public void Validate_NullFilter_AllowNullableTrue_ReturnsEmpty()
    {
        // Arrange
        FlagsEnumFilter<AccessRights>? filter = null;

        // Act
        var result = FlagsEnumValidator.Validate(filter!, allowNullable: true);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_NullFilter_AllowNullableFalse_ReturnsError()
    {
        // Arrange
        FlagsEnumFilter<AccessRights>? filter = null;

        // Act
        var result = FlagsEnumValidator.Validate(filter!, allowNullable: false);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("FlagsEnumFilter`1", error.Field);
        Assert.Equal("Filter must not be null", error.Error);
    }

    [Fact]
    public void Validate_EqAndNotEqSet_ReturnsError()
    {
        // Arrange
        var filter = new FlagsEnumFilter<AccessRights>
        {
            Eq = AccessRights.Read,
            NotEq = AccessRights.Delete
        };

        // Act
        var result = FlagsEnumValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("FlagsEnumFilter", error.Field);
        Assert.Equal("Cannot specify both Eq and NotEq simultaneously", error.Error);
    }

    [Fact]
    public void Validate_HasFlagAndNotHasFlagSetSame_ReturnsError()
    {
        // Arrange
        var filter = new FlagsEnumFilter<AccessRights>
        {
            HasFlag = AccessRights.Read,
            NotHasFlag = AccessRights.Read
        };

        // Act
        var result = FlagsEnumValidator.Validate(filter);

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("FlagsEnumFilter", error.Field);
        Assert.Equal("HasFlag and NotHasFlag cannot be equal (Read)", error.Error);
    }

    [Fact]
    public void Validate_CustomValidatorFails_ReturnsCustomError()
    {
        // Arrange
        var filter = new FlagsEnumFilter<AccessRights> { Eq = AccessRights.Read };

        // Act
        var result = FlagsEnumValidator.Validate(filter, customValidator: _ => (false, "custom failed"));

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("FlagsEnumFilter`1", error.Field);
        Assert.Equal("custom failed", error.Error);
    }

    [Fact]
    public void Validate_HasFlagAndNotHasFlagSetNotEqual_ReturnsEmpty()
    {
        // Arrange
        var filter = new FlagsEnumFilter<AccessRights>
        {
            HasFlag = AccessRights.Read,
            NotHasFlag = AccessRights.Write
        };

        // Act
        var result = FlagsEnumValidator.Validate(filter);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ValidFlags_ReturnsEmpty()
    {
        // Arrange
        var filter = new FlagsEnumFilter<AccessRights>
        {
            Eq = AccessRights.Read | AccessRights.Write
        };

        // Act
        var result = FlagsEnumValidator.Validate(filter);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_OnlyHasFlag_ReturnsEmpty()
    {
        // Arrange
        var filter = new FlagsEnumFilter<AccessRights>
        {
            HasFlag = AccessRights.Delete
        };

        // Act
        var result = FlagsEnumValidator.Validate(filter);

        // Assert
        Assert.Empty(result);
    }

    #region EnsureValid

    [Fact]
    public void EnsureValid_ValidFilter_DoesNotThrow()
    {
        // Arrange
        var filter = new FlagsEnumFilter<AccessRights>
        {
            HasFlag = AccessRights.Read,
            NotHasFlag = AccessRights.Delete
        };

        // Act
        var ex = Record.Exception(() =>
            FlagsEnumValidator.EnsureValid(filter, "files", "access"));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void EnsureValid_HasFlagEqualsNotHasFlag_ThrowsError()
    {
        // Arrange
        var filter = new FlagsEnumFilter<AccessRights>
        {
            HasFlag = AccessRights.Write,
            NotHasFlag = AccessRights.Write
        };

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            FlagsEnumValidator.EnsureValid(filter, "files", "access"));

        // Assert
        Assert.Contains("FlagsEnumFilter", ex.Message);
        Assert.Contains("table: 'files'", ex.Message);
        Assert.Contains("column: 'access'", ex.Message);
        Assert.Contains("HasFlag and NotHasFlag cannot be equal (Write)", ex.Message);
    }

    #endregion
}
