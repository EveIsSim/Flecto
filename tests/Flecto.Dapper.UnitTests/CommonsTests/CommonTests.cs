
using Flecto.Dapper.Commons;

namespace Flecto.Dapper.UnitTests.CommonTests;

public class CommonTests
{
    [Theory]
    [InlineData("users", "id", "users.id")]
    [InlineData("orders", "amount", "orders.amount")]
    [InlineData("data", "json->>'field'", "data.json->>'field'")]
    public void CombineColumn_ValidInputs_ReturnsCombinedString(
        string table,
        string column,
        string expected)
    {
        // Act
        var result = Common.CombineColumn(table, column);

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
