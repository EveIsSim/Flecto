using Flecto.Core.Validators;

namespace Flecto.Core.UnitTests.ValidatorTests;

public class TableColumnValidatorTests
{
    [Theory]
    [InlineData("users", "name")]
    [InlineData("user_data", "data->>'name'")]
    [InlineData("orders", "info->'total'")]
    [InlineData("t", "json->'field'->>'value'")]
    public void EnsureValidTableWithColumns_ValidInputs_DoesNotThrow(string table, string column)
    {
        // Arrange
        var input = new[] { (Table: table, Columns: new[] { column }) };

        // Act
        var ex = Record.Exception(() => TableColumnValidator.EnsureValidTableWithColumns(input));

        // Assert
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(null, new[] { "name" }, "Table name cannot be null or whitespace")]
    [InlineData(" ", new[] { "name" }, "Table name cannot be null or whitespace")]
    [InlineData("1table", new[] { "name" }, "Invalid Table name syntax: '1table'")]
    [InlineData("table", null, "Table 'table' must have at least one column specified")]
    [InlineData("table", new string[0], "Table 'table' must have at least one column specified")]
    [InlineData("table", new[] { "" }, "Column name cannot be null or whitespace")]
    [InlineData("table", new[] { " " }, "Column name cannot be null or whitespace")]
    [InlineData("table", new[] { "->invalid" }, "Invalid Column name syntax: '->invalid'")]
    [InlineData("table", new[] { "column->'bad space'" }, "Invalid Column name syntax: 'column->'bad space''")]
    public void EnsureValidTableWithColumns_InvalidInputs_Throws(string? table, string[]? columns, string expectedMessage)
    {
        // Arrange
        var input = new[] { (Table: table, Columns: columns) };

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
                TableColumnValidator.EnsureValidTableWithColumns(input!));

        // Assert
        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void EnsureValidTableWithColumns_NullInput_Throws()
    {
        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
                TableColumnValidator.EnsureValidTableWithColumns(null!));

        // Assert
        Assert.Equal("At least one table with columns must be specified", ex.Message);
    }

    [Fact]
    public void EnsureValidTableWithColumns_EmptyInput_Throws()
    {
        // Act
        var ex = Assert.Throws<ArgumentException>(()
                => TableColumnValidator.EnsureValidTableWithColumns());

        // Assert
        Assert.Equal("At least one table with columns must be specified", ex.Message);
    }
}
