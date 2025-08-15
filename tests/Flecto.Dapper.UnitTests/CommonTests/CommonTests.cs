
using Flecto.Dapper.Commons;
using Flecto.Dapper.Models;
using Flecto.Dapper.SqlDialect.Dialects.Postgres.Casting;

namespace Flecto.Dapper.UnitTests.CommonTests;

public class CommonTests
{
    [Theory]
    [InlineData("users", "id", "users.id", typeof(long))]
    [InlineData("orders", "amount", "orders.amount", typeof(decimal))]
    [InlineData("data", "json->>'field'", "(data.json->>'field')::text", typeof(string))]
    public void ColumnRef_SqlName_ValidInputs_ReturnsCombinedString(
        string table,
        string column,
        string expected,
        Type clrType)
    {
        // Act
        var cr = new ColumnRef(table, column, 0, new PgCastTypeMapper());
        var result = cr.SqlName(clrType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("p", 0, "p0")]
    [InlineData("param", 5, "param5")]
    [InlineData("_tmp", 123, "_tmp123")]
    public void GenSearchParamName_ValidInputs_ReturnsCombinedName(
        string prefix,
        int counter,
        string expected)
    {
        // Act
        var result = Common.GenSearchParamName(prefix, counter);

        // Assert
        Assert.Equal(expected, result);
    }

}
