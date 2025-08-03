using Flecto.Core.Models.Select;
using Flecto.Core.Validators;

namespace Flecto.Core.UnitTests.ValidatorTests;

public class TableColumnValidatorTests
{

    #region EnsureValidTableWithColumns

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

    #endregion

    #region EnsureValidSelectTableWithColumns

    [Theory]
    [InlineData("users", "name", null)]
    [InlineData("user_data", "data->>'name'", "data_name")]
    [InlineData("orders", "info->'total'", "info_total")]
    [InlineData("t", "json->'field'->>'value'", "json_field_value")]
    public void EnsureValidSelectTableWithColumns_ValidInputs_DoesNotThrow(
        string table,
        string column,
        string? alias)
    {
        // Arrange
        var input = new FromTable[] { new(table, new Field[] { new(column, alias) }) };

        // Act
        var ex = Record.Exception(() => TableColumnValidator.EnsureValidSelectTableWithColumns(input));

        // Assert
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(null, new[] { "name" }, null, "Table name cannot be null or whitespace")]
    [InlineData(" ", new[] { "name" }, null, "Table name cannot be null or whitespace")]
    [InlineData("1table", new[] { "name" }, null, "Invalid Table name syntax: '1table'")]
    [InlineData("table", null, null, "Table 'table' must have at least one column specified")]
    [InlineData("table", new string[0], null, "Table 'table' must have at least one column specified")]
    [InlineData("table", new[] { "" }, null, "Column name cannot be null or whitespace")]
    [InlineData("table", new[] { " " }, null, "Column name cannot be null or whitespace")]
    [InlineData("table", new[] { "->invalid" }, null, "Invalid Column name syntax: '->invalid'")]
    [InlineData("table", new[] { "column->'bad space'" }, null, "Invalid Column name syntax: 'column->'bad space''")]
    [InlineData("table", new[] { "name" }, "1alias", "Invalid Alias name syntax: '1alias'")]
    [InlineData("table", new[] { "name" }, "bad alias", "Invalid Alias name syntax: 'bad alias'")]
    public void EnsureValidSelectTableWithColumns_InvalidInputs_Throws(
        string? table,
        string[]? columns,
        string? alias,
        string expectedMessage)
    {
        // Arrange
        var fields = columns?
            .Select((col) => new Field(col, alias))
            .ToArray()
            ?? Array.Empty<Field>();

        var input = new[] { new FromTable(table!, fields) };

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            TableColumnValidator.EnsureValidSelectTableWithColumns(input!));

        // Assert
        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void EnsureValidSelectTableWithColumns_NullInput_Throws()
    {
        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
                TableColumnValidator.EnsureValidSelectTableWithColumns(null!));

        // Assert
        Assert.Equal("At least one table with columns must be specified", ex.Message);
    }

    [Fact]
    public void EnsureValidSelectTableWithColumns_EmptyInput_Throws()
    {
        // Act
        var ex = Assert.Throws<ArgumentException>(()
                => TableColumnValidator.EnsureValidSelectTableWithColumns([]));

        // Assert
        Assert.Equal("At least one table with columns must be specified", ex.Message);
    }

    #endregion
}
