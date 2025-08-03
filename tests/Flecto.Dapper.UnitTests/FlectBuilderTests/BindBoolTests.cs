
using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Select;

namespace Flecto.Dapper.UnitTests.FlectoBuilderTests;

public class BindBoolTests
{
    private const string Table = "users";

    [Fact]
    public void BindBool_FilterIsNull_NotAddCondition()
    {
        // Arrange
        BoolFilter? filter = null;
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        var tc = new FromTable(Table, new Field[] { new("id") });

        // Act
        var result = builder
            .Select(tc)
            .BindBool(filter, "is_active")
            .Build();

        // Assert
        Assert.Equal("SELECT users.id FROM users", result.Sql);
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindBool_ValidationFailes_ThrowException()
    {
        // Arrange
        var filter = new BoolFilter { Eq = true, NotEq = true };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);
        var tc = new FromTable(Table, new Field[] { new("id") });

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            builder
            .Select(tc)
            .BindBool(filter, "is_active")
            .Build());

        // Assert
        var expected = """
            BoolFilter: validation for table: 'users', column: 'is_active' failed:
            BoolFilter: Cannot specify both Eq and NotEq simultaneously
            """;
        Assert.Equal(expected, ex.Message);
    }

    [Fact]
    public void BindBool_WithEqTrue_AddsEqualCondition()
    {
        // Arrange
        var filter = new BoolFilter { Eq = true };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);
        var tc = new FromTable(Table, new Field[] { new("id") });

        // Act
        var result = builder
            .Select(tc)
            .BindBool(filter, "is_active")
            .Build();

        // Assert
        var expectedParam = "users_is_active_Eq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.is_active = @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<bool>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.Eq, paramDict[expectedParam]);
    }

    [Fact]
    public void BindBool_MultiBind_AddConditions()
    {
        // Arrange
        var filter0 = new BoolFilter { Eq = true };
        var filter1 = new BoolFilter { Eq = false };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);
        var tc = new FromTable(Table, new Field[] { new("id") });

        // Act
        var result = builder
            .Select(tc)
            .BindBool(filter0, "is_active")
            .BindBool(filter1, "is_active")
            .Build();

        // Assert
        var expectedParam0 = "users_is_active_Eq_0";
        var expectedParam1 = "users_is_active_Eq_1";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.is_active = @{expectedParam0} " +
                $"AND users.is_active = @{expectedParam1}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<bool>(name));

        Assert.Equal(2, paramDict.Count());
        Assert.True(paramDict.ContainsKey(expectedParam0));
        Assert.Equal(filter0.Eq, paramDict[expectedParam0]);

        Assert.True(paramDict.ContainsKey(expectedParam1));
        Assert.Equal(filter1.Eq, paramDict[expectedParam1]);
    }

    [Fact]
    public void BindBool_WithNotEqFalse_AddsNotEqualCondition()
    {
        // Arrange
        var filter = new BoolFilter { NotEq = false };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);
        var tc = new FromTable(Table, new Field[] { new("id") });

        // Act
        var result = builder
            .Select(tc)
            .BindBool(filter, "is_active")
            .Build();

        // Assert
        var expectedParam = "users_is_active_NotEq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.is_active <> @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<bool>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.NotEq, paramDict[expectedParam]);
    }

    [Fact]
    public void BindBool_WithIsNullTrue_AddsIsNullCheck()
    {
        // Arrange
        var filter = new BoolFilter { IsNull = true };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);
        var tc = new FromTable(Table, new Field[] { new("id") });

        // Act
        var result = builder
            .Select(tc)
            .BindBool(filter, "is_active")
            .Build();

        // Assert
        Assert.Equal("SELECT users.id FROM users WHERE users.is_active IS NULL", result.Sql);
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindBool_WithIsNullFalse_AddsIsNotNullCheck()
    {
        // Arrange
        var filter = new BoolFilter { IsNull = false };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);
        var tc = new FromTable(Table, new Field[] { new("id") });

        // Act
        var result = builder
            .Select(tc)
            .BindBool(filter, "is_active")
            .Build();

        // Assert
        Assert.Equal("SELECT users.id FROM users WHERE users.is_active IS NOT NULL", result.Sql);
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindBool_WithSort_AddsOrderBy()
    {
        // Arrange
        var filter = new BoolFilter
        {
            Sort = new Sort(position: 1, descending: true)
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);
        var tc = new FromTable(Table, new Field[] { new("id") });

        // Act
        var result = builder
            .Select(tc)
            .BindBool(filter, "is_active")
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id FROM users ORDER BY users.is_active DESC",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindBool_JsonbColumn_EqTrue_GeneratesCorrectCondition()
    {
        // Arrange
        var filter = new BoolFilter { Eq = true };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);
        var tc = new FromTable(Table, new Field[] { new("id") });

        // Act
        var result = builder
            .Select(tc)
            .BindBool(filter, "profile->'is_active'")
            .Build();

        // Assert
        var expectedParam = "users_profile_is_active_Eq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE (users.profile->'is_active')::boolean = @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<bool>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.Eq, paramDict[expectedParam]);
    }
}
