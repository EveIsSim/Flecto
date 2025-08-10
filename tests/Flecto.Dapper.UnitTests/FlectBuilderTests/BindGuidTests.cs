using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Select;

namespace Flecto.Dapper.UnitTests.FlectoBuilderTests;

public class BindGuidTests
{
    private const string Table = "users";
    private const string Column = "department_id";
    private readonly FromTable _tc = new FromTable(Table, new Field[] { new("id") });

    [Fact]
    public void BindGuid_FilterIsNull_NotAddCondition()
    {
        // Arrange
        GuidFilter? filter = null;
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindGuid(filter, Column)
            .Build();

        // Assert
        Assert.Equal("SELECT users.id FROM users", result.Sql);
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindGuid_ValidationFailes_ThrowException()
    {
        // Arrange
        var filter = new GuidFilter
        {
            Eq = Guid.NewGuid(),
            NotEq = Guid.NewGuid()
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            builder
            .Select(_tc)
            .BindGuid(filter, Column)
            .Build());

        // Assert
        var expected = """
            GuidFilter: validation for table: 'users', column: 'department_id' failed:
            GuidFilter: Cannot specify both Eq and NotEq simultaneously
            """;
        Assert.Equal(expected, ex.Message);
    }

    [Fact]
    public void BindGuid_WithEq_AddsEqualCondition()
    {
        // Arrange
        var filter = new GuidFilter
        {
            Eq = Guid.NewGuid()
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindGuid(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_department_id_Eq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.department_id = @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<Guid>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.Eq, paramDict[expectedParam]);
    }

    [Fact]
    public void BindGuid_WithNotEq_AddsNotEqualCondition()
    {
        // Arrange
        var filter = new GuidFilter
        {
            NotEq = Guid.NewGuid()
        };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindGuid(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_department_id_NotEq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.department_id <> @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<Guid>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.NotEq, paramDict[expectedParam]);
    }

    [Fact]
    public void BindGuid_InArray_AddsIn()
    {
        // Arrange
        var filter = new GuidFilter
        {
            In = [
                Guid.NewGuid(),
                Guid.NewGuid()
            ]
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindGuid(filter, Column)
            .Build();

        // Assert
        var expectedIn = "users_department_id_In_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.department_id = ANY(@{expectedIn})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<Guid[]>(name));

        Assert.Single(paramDict);
        Assert.Equal(filter.In, paramDict[expectedIn]);
    }

    [Fact]
    public void BindGuid_NotInArray_AddsNotIn()
    {
        // Arrange
        var filter = new GuidFilter
        {
            NotIn = [
                Guid.NewGuid(),
                Guid.NewGuid()
            ]
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindGuid(filter, Column)
            .Build();

        // Assert
        var expectedIn = "users_department_id_NotIn_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.department_id <> ANY(@{expectedIn})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<Guid[]>(name));

        Assert.Single(paramDict);
        Assert.Equal(filter.NotIn, paramDict[expectedIn]);
    }

    [Fact]
    public void BindGuid_IsNullTrue_AddsIsNull()
    {
        // Arrange
        var filter = new GuidFilter { IsNull = true };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindGuid(filter, Column)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "WHERE users.department_id IS NULL",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindGuid_IsNullFalse_AddsIsNotNull()
    {
        // Arrange
        var filter = new GuidFilter { IsNull = false };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindGuid(filter, Column)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "WHERE users.department_id IS NOT NULL",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindGuid_WithSort_AddsOrderBy()
    {
        // Arrange
        var filter = new GuidFilter
        {
            Sort = new Sort(position: 1, descending: true)
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindGuid(filter, Column)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id FROM users ORDER BY users.department_id DESC",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindGuid_JsonbColumn_Eq_GeneratesCorrectCondition()
    {
        // Arrange
        var filter = new GuidFilter
        {
            Eq = Guid.NewGuid(),
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindGuid(filter, "profile->>'department_id'")
            .Build();

        // Assert
        var expectedParam = "users_profile_department_id_Eq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE (users.profile->>'department_id')::uuid = @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<Guid>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.Eq, paramDict[expectedParam]);
    }

    [Fact]
    public void BindGuid_MultiBind_AddConditions()
    {
        // Arrange
        var filter0 = new GuidFilter { Eq = Guid.NewGuid() };
        var filter1 = new GuidFilter { Eq = Guid.NewGuid() };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindGuid(filter0, Column)
            .BindGuid(filter1, Column)
            .Build();

        // Assert
        var expectedParam0 = "users_department_id_Eq_0";
        var expectedParam1 = "users_department_id_Eq_1";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.department_id = @{expectedParam0} " +
                $"AND users.department_id = @{expectedParam1}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<Guid>(name));

        Assert.Equal(2, paramDict.Count());
        Assert.True(paramDict.ContainsKey(expectedParam0));
        Assert.Equal(filter0.Eq, paramDict[expectedParam0]);

        Assert.True(paramDict.ContainsKey(expectedParam1));
        Assert.Equal(filter1.Eq, paramDict[expectedParam1]);
    }
}
