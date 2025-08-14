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

    [Fact]
    public void BindString_InArray_CaseSensitive_True()
    {
        // Arrange
        var filter = new StringFilter
        {
            In = ["Anna", "Bob"],
            CaseSensitive = true
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindString(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_name_In_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.name = ANY(@{expectedParam})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<string[]>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.In, paramDict[expectedParam]);
    }

    [Fact]
    public void BindString_InArray_CaseSensitive_False()
    {
        // Arrange
        var filter = new StringFilter
        {
            In = ["Anna", "Bob"],
            CaseSensitive = false
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindString(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_name_In_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE LOWER(users.name) = ANY(@{expectedParam})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<string[]>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(new[] { "anna", "bob" }, paramDict[expectedParam]);
    }

    [Fact]
    public void BindString_NotInArray_CaseSensitive_True()
    {
        // Arrange
        var filter = new StringFilter
        {
            NotIn = ["Anna", "Bob"],
            CaseSensitive = true
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindString(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_name_NotIn_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.name <> ALL(@{expectedParam})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<string[]>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.NotIn, paramDict[expectedParam]);
    }

    [Fact]
    public void BindString_NotInArray_CaseSensitive_False()
    {
        // Arrange
        var filter = new StringFilter
        {
            NotIn = ["Anna", "Bob"],
            CaseSensitive = false
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindString(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_name_NotIn_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE LOWER(users.name) <> ALL(@{expectedParam})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<string[]>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(new[] { "anna", "bob" }, paramDict[expectedParam]);
    }

    [Fact]
    public void BindString_IsNullTrue_AddsIsNull()
    {
        // Arrange
        var filter = new StringFilter { IsNull = true };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindString(filter, Column)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "WHERE users.name IS NULL",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindString_IsNullFalse_AddsIsNotNull()
    {
        // Arrange
        var filter = new StringFilter { IsNull = false };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindString(filter, Column)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "WHERE users.name IS NOT NULL",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindString_WithSort_AddsOrderBy()
    {
        // Arrange
        var filter = new StringFilter
        {
            Sort = new Sort(position: 1, descending: true)
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindString(filter, Column)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id FROM users ORDER BY users.name DESC",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindString_MultiBind_AddConditions()
    {
        // Arrange
        var filter0 = new StringFilter { Eq = "Alice" };
        var filter1 = new StringFilter { Eq = "Bob" };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindString(filter0, Column)
            .BindString(filter1, Column)
            .Build();

        // Assert
        var expectedParam0 = "users_name_Eq_0";
        var expectedParam1 = "users_name_Eq_1";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.name = @{expectedParam0} " +
                $"AND users.name = @{expectedParam1}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<string>(name));

        Assert.Equal(2, paramDict.Count());
        Assert.True(paramDict.ContainsKey(expectedParam0));
        Assert.Equal(filter0.Eq, paramDict[expectedParam0]);

        Assert.True(paramDict.ContainsKey(expectedParam1));
        Assert.Equal(filter1.Eq, paramDict[expectedParam1]);
    }

    [Fact]
    public void BindString_JsonbColumn_Eq_GeneratesCorrectCondition()
    {
        // Arrange
        var filter = new StringFilter
        {
            In = ["Alice", "Bob"],
            CaseSensitive = false
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindString(filter, "profile->>'name'")
            .Build();

        // Assert
        var expectedParam = "users_profile_name_In_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE LOWER((users.profile->>'name')::text) = ANY(@{expectedParam})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<string[]>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(new string[] { "alice", "bob" }, paramDict[expectedParam]);
    }
}
