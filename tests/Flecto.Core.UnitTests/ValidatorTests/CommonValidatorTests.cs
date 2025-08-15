using Flecto.Core.Models.Filters;
using Flecto.Core.UnitTests.Collections;
using Flecto.Core.Validators;

namespace Flecto.Core.UnitTests.ValidatorTests;

[Collection(CollectionConsts.TestColletionName)]
public class CommonValidatorTests
{
    private class DummyFilter : IFilter { }
    private readonly BoolFilter _defaultFilter = new();

    #region ValidateNullOr

    [Fact]
    public void ValidateNullOr_AllowNullableTrue_ReturnEmpty()
    {
        // Arrange
        DummyFilter? filter = null;

        // Act
        var result = CommonValidator.ValidateNullOr(
            filter,
            allowNullable: true,
            validateNotNull: static _ => throw new ArgumentException("Should not be called"));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateNullOr_FilterIsNull_AllowNullableFalse_ReturnsError()
    {
        // Arrange
        DummyFilter? filter = null;

        // Act
        var result = CommonValidator.ValidateNullOr(
            filter,
            allowNullable: false,
            validateNotNull: static _ => throw new ArgumentException("Should not be called"));

        // Assert
        var error = result.Single();
        Assert.Equal("DummyFilter", error.Field);
        Assert.Equal("Filter must not be null", error.Error);
    }

    [Fact]
    public void ValidateNullOr_FilterIsNotNull_UsesValidateNotNull()
    {
        // Act
        var result = CommonValidator.ValidateNullOr(
            _defaultFilter,
            allowNullable: true,
            validateNotNull: static f =>
            {
                return [("FieldX", "Some error")];
            });

        // Assert
        var error = result.Single();
        Assert.Equal("FieldX", error.Field);
        Assert.Equal("Some error", error.Error);
    }

    #endregion

    #region ValidateEqAndNotEq

    [Fact]
    public void ValidateEqAndNotEq_BothNull_ReturnsEmpty()
    {
        // Act
        var result = CommonValidator.ValidateEqAndNotEq<bool?>(null, null, "MyField");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateEqAndNotEq_OnlyEqSpecified_ReturnsEmpty()
    {
        // Act
        var result = CommonValidator.ValidateEqAndNotEq<bool?>(true, null, "MyField");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateEqAndNotEq_OnlyNotEqSpecified_ReturnsEmpty()
    {
        // Act
        var result = CommonValidator.ValidateEqAndNotEq<bool?>(null, false, "MyField");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateEqAndNotEq_BothSpecified_ReturnsError()
    {
        // Act
        var result = CommonValidator.ValidateEqAndNotEq<bool?>(true, false, "MyField");

        // Assert
        var error = Assert.Single(result);

        Assert.Equal("MyField", error.Field);
        Assert.Equal("Cannot specify both Eq and NotEq simultaneously", error.Error);
    }

    #endregion

    #region ValidateArrayIfNeeded

    [Fact]
    public void ValidateArrayIfNeeded_NullArray_ReturnsEmpty()
    {
        // Arrange
        int[]? arr = null;

        // Act
        var result = CommonValidator.ValidateArrayIfNeeded(arr, "MyField");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateArrayIfNeeded_EmptyArray_ReturnsError()
    {
        // Arrange
        var arr = Array.Empty<int>();

        // Act
        var result = CommonValidator.ValidateArrayIfNeeded(arr, "MyField");

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("MyField", error.Field);
        Assert.Equal("Array cannot be empty if specified", error.Error);
    }

    [Fact]
    public void ValidateArrayIfNeeded_UniqueArray_ReturnsEmpty()
    {
        // Arrange
        var arr = new[] { 1, 2, 3 };

        // Act
        var result = CommonValidator.ValidateArrayIfNeeded(arr, "MyField");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateArrayIfNeeded_ArrayWithDuplicates_ReturnsError()
    {
        // Arrange
        var arr = new[] { 1, 2, 2, 3 };

        // Act
        var result = CommonValidator.ValidateArrayIfNeeded(arr, "MyField");

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("MyField", error.Field);
        Assert.Equal("Array contains duplicate values", error.Error);
    }

    [Fact]
    public void ValidateArrayIfNeeded_ArrayWithAnyNull_ReturnsError()
    {
        // Arrange
        var arr = new[] { "Alice", null };

        // Act
        var result = CommonValidator.ValidateArrayIfNeeded(arr, "MyField");

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("MyField", error.Field);
        Assert.Equal("Array contains null values", error.Error);
    }

    #endregion

    #region ValidateViaCustomValidatorIfNeeded

    [Fact]
    public void ValidateViaCustomValidatorIfNeeded_NoCustomValidator_YieldsNothing()
    {
        // Act
        var result = CommonValidator.ValidateViaCustomValidatorIfNeeded(
            _defaultFilter,
            customValidator: null);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateViaCustomValidatorIfNeeded_CustomValidatorPasses_YieldsNothing()
    {
        // Act
        var result = CommonValidator.ValidateViaCustomValidatorIfNeeded(
            _defaultFilter,
            static f => (true, null));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateViaCustomValidatorIfNeeded_CustomValidatorFails_YieldsError()
    {
        // Act
        var result = CommonValidator.ValidateViaCustomValidatorIfNeeded(
            _defaultFilter,
            static f => (false, "Custom validation failed"));

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("BoolFilter", error.Field);
        Assert.Equal("Custom validation failed", error.Error);
    }

    [Fact]
    public void ValidateViaCustomValidatorIfNeeded_CustomValidatorFails_NullMessage_UsesDefault()
    {
        // Act
        var result = CommonValidator.ValidateViaCustomValidatorIfNeeded(
            _defaultFilter,
            static f => (false, null));

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("BoolFilter", error.Field);
        Assert.Equal("Filter failed custom validation", error.Error);
    }

    #endregion

    #region ThrowIfErrors

    [Fact]
    public void ThrowIfErrors_EmptyErrors_DoesNothing()
    {
        // Arrange
        var errors = Array.Empty<(string Field, string Error)>();

        // Act
        var ex = Record.Exception(() => CommonValidator.ThrowIfErrors(errors));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void ThrowIfErrors_ThrowsWithSingleError_NoPrefix()
    {
        // Arrange
        var errors = new[] { ("MyField", "Something went wrong") };

        // Act
        var ex = Assert.Throws<ArgumentException>(() => CommonValidator.ThrowIfErrors(errors));

        // Assert
        Assert.Equal("MyField: Something went wrong", ex.Message);
    }

    [Fact]
    public void ThrowIfErrors_ThrowsWithMultipleErrors_NoPrefix()
    {
        // Arrange
        var errors = new[]
        {
            ("Field1", "Err1"),
            ("Field2", "Err2")
        };

        // Act
        var ex = Assert.Throws<ArgumentException>(() => CommonValidator.ThrowIfErrors(errors));

        // Assert
        var expected = "Field1: Err1\nField2: Err2";
        Assert.Equal(expected, ex.Message);
    }

    [Fact]
    public void ThrowIfErrors_ThrowsWithPrefix()
    {
        // Arrange
        var errors = new[] { ("FieldX", "Invalid value") };

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            CommonValidator.ThrowIfErrors(errors, prefix: "Validation failed"));

        // Assert
        var expected = """
            Validation failed
            FieldX: Invalid value
            """;
        Assert.Equal(expected, ex.Message);
    }

    [Fact]
    public void ThrowIfErrors_IgnoresEmptyPrefix()
    {
        // Arrange
        var errors = new[] { ("F", "E") };

        // Act
        var ex1 = Assert.Throws<ArgumentException>(() => CommonValidator.ThrowIfErrors(errors, ""));
        var ex2 = Assert.Throws<ArgumentException>(() => CommonValidator.ThrowIfErrors(errors, "   "));

        // Assert
        Assert.Equal("F: E", ex1.Message);
        Assert.Equal("F: E", ex2.Message);
    }

    #endregion

    #region EnsureValidBindFilter
    [Fact]
    public void EnsureValidBindFilter_AllValid_DoesNotThrow()
    {
        // Act
        var ex = Record.Exception(() =>
            CommonValidator.EnsureValidBindFilter(
                _defaultFilter,
                "users",
                "is_active",
                _ => []));

        // Assert
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureValidBindFilter_InvalidTable_Throws(string? table)
    {
        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            CommonValidator.EnsureValidBindFilter(
                _defaultFilter,
                table!,
                "status",
                _ => []));

        // Assert
        Assert.Equal("Table name cannot be null or whitespace", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureValidBindFilter_InvalidColumn_Throws(string? column)
    {
        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            CommonValidator.EnsureValidBindFilter(
                _defaultFilter,
                "orders",
                column!,
                _ => []));

        // Assert
        Assert.Equal("Column name cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void EnsureValidBindFilter_ValidationFails_ThrowsWithDetailedMessage()
    {
        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            CommonValidator.EnsureValidBindFilter(
                _defaultFilter,
                "logs",
                "enabled",
                _ => [("SomeField", "SomeError")]));

        // Assert
        Assert.Equal(
            """
            BoolFilter: validation for table: 'logs', column: 'enabled' failed:
            SomeField: SomeError
            """,
            ex.Message);
    }

    #endregion

    #region ValidateRangeConsistency

    [Fact]
    public void ValidateRangeConsistency_AllNull_ReturnsEmpty()
    {
        // Act
        var result = CommonValidator.ValidateRangeConsistency<int>(
            gt: null, gte: null, lt: null, lte: null, filterName: "Range");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateRangeConsistency_GtGreaterOrEqualToLt_ReturnsError()
    {
        // Act
        var result = CommonValidator.ValidateRangeConsistency<int>(
            gt: 10, gte: null, lt: 10, lte: null, filterName: "MyField");

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("MyField", error.Field);
        Assert.Equal("Gt (10) must be less than Lt (10)", error.Error);
    }

    [Fact]
    public void ValidateRangeConsistency_GtGreaterThanLte_ReturnsError()
    {
        // Act
        var result = CommonValidator.ValidateRangeConsistency<int>(
            gt: 11, gte: null, lt: null, lte: 10, filterName: "MyField");

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("MyField", error.Field);
        Assert.Equal("Gt (11) must be less than or equal to Lte (10)", error.Error);
    }

    [Fact]
    public void ValidateRangeConsistency_GteGreaterOrEqualToLt_ReturnsError()
    {
        // Act
        var result = CommonValidator.ValidateRangeConsistency<int>(
            gt: null, gte: 15, lt: 10, lte: null, filterName: "MyField");

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("MyField", error.Field);
        Assert.Equal("Gte (15) must be less than Lt (10)", error.Error);
    }

    [Fact]
    public void ValidateRangeConsistency_GteGreaterThanLte_ReturnsError()
    {
        // Act
        var result = CommonValidator.ValidateRangeConsistency<int>(
            gt: null, gte: 21, lt: null, lte: 20, filterName: "MyField");

        // Assert
        var error = Assert.Single(result);
        Assert.Equal("MyField", error.Field);
        Assert.Equal("Gte (21) must be less than or equal to Lte (20)", error.Error);
    }

    [Fact]
    public void ValidateRangeConsistency_ValidRanges_ReturnsEmpty()
    {
        // Act
        var result = CommonValidator.ValidateRangeConsistency<int>(
            gt: 5, gte: 3, lt: 10, lte: 15, filterName: "Range");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateRangeConsistency_MultipleViolations_ReturnsAllErrors()
    {
        // Act
        var result = CommonValidator.ValidateRangeConsistency<int>(
            gt: 10, gte: 20, lt: 9, lte: 8, filterName: "Fail");

        // Assert
        var errors = result.ToArray();

        Assert.Equal(4, errors.Length);
        Assert.Contains(errors, static e => e.Error == "Gt (10) must be less than Lt (9)");
        Assert.Contains(errors, static e => e.Error == "Gt (10) must be less than or equal to Lte (8)");
        Assert.Contains(errors, static e => e.Error == "Gte (20) must be less than Lt (9)");
        Assert.Contains(errors, static e => e.Error == "Gte (20) must be less than or equal to Lte (8)");
    }

    #endregion
}
