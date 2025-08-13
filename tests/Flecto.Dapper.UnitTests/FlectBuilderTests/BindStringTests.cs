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

    [Theory]
    [InlineData(true, "=")]
    [InlineData(false, "ILIKE")]
    public void BindString_Eq_SensitiveCases(bool caseSensitive, string operation)
    {
        // Arrange
        var filter = new StringFilter
        {
            Eq = "Alice",
            CaseSensitive = caseSensitive
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindString(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_name_Eq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.name {operation} @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<string>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.Eq, paramDict[expectedParam]);
    }

    [Theory]
    [InlineData(true, "<>")]
    [InlineData(false, "NOT ILIKE")]
    public void BindString_NotEq_SensitiveCases(bool caseSensitive, string operation)
    {
        // Arrange
        var filter = new StringFilter
        {
            NotEq = "Alice",
            CaseSensitive = caseSensitive
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindString(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_name_NotEq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.name {operation} @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<string>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.NotEq, paramDict[expectedParam]);
    }

    [Theory]
    [InlineData(true, "LIKE")]
    [InlineData(false, "ILIKE")]
    public void BindString_Contains_SensitiveCases(bool caseSensitive, string operation)
    {
        // Arrange
        var filter = new StringFilter
        {
            Contains = "Ali",
            CaseSensitive = caseSensitive
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindString(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_name_Contains_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.name {operation} @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<string>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal($"%{filter.Contains}%", paramDict[expectedParam]);
    }

    [Theory]
    [InlineData(true, "LIKE")]
    [InlineData(false, "ILIKE")]
    public void BindString_StartsWith_SensitiveCases(bool caseSensitive, string operation)
    {
        // Arrange
        var filter = new StringFilter
        {
            StartsWith = "Ali",
            CaseSensitive = caseSensitive
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindString(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_name_StartsWith_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.name {operation} @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<string>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal($"{filter.StartsWith}%", paramDict[expectedParam]);
    }

    [Theory]
    [InlineData(true, "LIKE")]
    [InlineData(false, "ILIKE")]
    public void BindString_EndsWith_SensitiveCases(bool caseSensitive, string operation)
    {
        // Arrange
        var filter = new StringFilter
        {
            EndsWith = "ICE",
            CaseSensitive = caseSensitive
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindString(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_name_EndsWith_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.name {operation} @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<string>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal($"%{filter.EndsWith}", paramDict[expectedParam]);
    }

    // 999 we have problem here, need fix sql build for In, NotIn with caseSansitive
    //[Theory]
    //[InlineData(true, "= ANY", new string[] { "Anna", "Bob" })]
    //[InlineData(false, "= ANY", new string[] { "anna", "bob" })]
    //public void BindString_InArray_SensitiveCases(
    //    bool caseSensitive,
    //    string operation,
    //    string[] expectedValue)
    //{
    //    // Arrange
    //    var filter = new StringFilter
    //    {
    //        In = ["Anna", "Bob"],
    //        CaseSensitive = caseSensitive
    //    };

    //    var builder = new FlectoBuilder(Table, DialectType.Postgres);

    //    // Act
    //    var result = builder
    //        .Select(_tc)
    //        .BindString(filter, Column)
    //        .Build();

    //    // Assert
    //    var expectedParam = "users_name_In_0";

    //    Assert.Equal(
    //        "SELECT users.id " +
    //        "FROM users " +
    //        $"WHERE users.name {operation}(@{expectedParam})",
    //        result.Sql
    //    );

    //    var paramDict = result.Parameters.ParameterNames
    //        .ToDictionary(
    //            name => name,
    //            name => result.Parameters.Get<string[]>(name));

    //    Assert.Single(paramDict);
    //    Assert.True(paramDict.ContainsKey(expectedParam));
    //    Assert.Equal(expectedValue, paramDict[expectedParam]);
    //}
}
