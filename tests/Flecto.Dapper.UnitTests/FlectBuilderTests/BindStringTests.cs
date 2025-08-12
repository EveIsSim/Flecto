using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Select;

namespace Flecto.Dapper.UnitTests.FlectoBuilderTests;

public class BindStringTests
{
    private const string Table = "users";
    private const string Column = "name";
    private readonly FromTable _tc = new FromTable(Table, new Field[] { new("id") });

    [Fact]
    public void BindString_FilterIsNull_NotAddCondition()
    {
        // Arrange
        StringFilter? filter = null;
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindString(filter, Column)
            .Build();

        // Assert
        Assert.Equal("SELECT users.id FROM users", result.Sql);
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindString_ValidationFailes_ThrowException()
    {
        // Arrange
        var filter = new StringFilter
        {
            In = ["hello", "hello"],
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            builder
            .Select(_tc)
            .BindString(filter, Column)
            .Build());

        // Assert
        var expected = """
            StringFilter: validation for table: 'users', column: 'name' failed:
            In: Array contains duplicate values
            """;
        Assert.Equal(expected, ex.Message);
    }
}
