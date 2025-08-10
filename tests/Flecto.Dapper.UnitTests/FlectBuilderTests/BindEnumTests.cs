
using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Filters.Enums;
using Flecto.Core.Models.Select;

namespace Flecto.Dapper.UnitTests.FlectoBuilderTests;

public class BindEnumTests
{
    private const string Table = "users";
    private const string Column = "status";
    private readonly FromTable _tc = new FromTable(Table, new Field[] { new("id") });

    private enum UserStatus
    {
        Unknown = 0,
        Active = 1,
        Inactive = 2
    }

    [Fact]
    public void BindEnum_FilterIsNull_NotAddCondition()
    {
        // Arrange
        EnumFilter<UserStatus>? filter = null;
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindEnum<UserStatus>(filter, Column, EnumFilterMode.Name)
            .Build();

        // Assert
        Assert.Equal("SELECT users.id FROM users", result.Sql);
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindEnum_ValidationFailes_ThrowException()
    {
        // Arrange
        var filter = new EnumFilter<UserStatus>
        {
            Eq = UserStatus.Active,
            NotEq = UserStatus.Inactive,
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            builder
            .Select(_tc)
            .BindEnum<UserStatus>(filter, Column, EnumFilterMode.Name)
            .Build());

        // Assert
        var expected = """
            EnumFilter`1: validation for table: 'users', column: 'status' failed:
            EnumFilter: Cannot specify both Eq and NotEq simultaneously
            """;
        Assert.Equal(expected, ex.Message);
    }

    [Theory]
    [InlineData(EnumFilterMode.Name, "Active", typeof(string))]
    [InlineData(EnumFilterMode.Value, 1, typeof(int))]
    [InlineData(EnumFilterMode.ValueString, "1", typeof(string))]
    public void BindEnum_WithEq_AddsEqualCondition(
        EnumFilterMode mode,
        object expectedValue,
        Type expectedType)
    {
        // Arrange
        var filter = new EnumFilter<UserStatus>
        {
            Eq = UserStatus.Active
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindEnum<UserStatus>(filter, Column, mode)
            .Build();

        // Assert
        var expectedParam = "users_status_Eq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.status = @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<object>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));

        var convertedValue = Convert.ChangeType(paramDict[expectedParam], expectedType);
        Assert.Equal(expectedValue, convertedValue);
    }

    [Theory]
    [InlineData(EnumFilterMode.Name, "Active", typeof(string))]
    [InlineData(EnumFilterMode.Value, 1, typeof(int))]
    [InlineData(EnumFilterMode.ValueString, "1", typeof(string))]
    public void BindEnum_WithNotEq_AddsNotEqualCondition(
        EnumFilterMode mode,
        object expectedValue,
        Type expectedType)
    {
        // Arrange
        var filter = new EnumFilter<UserStatus>
        {
            NotEq = UserStatus.Active
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindEnum<UserStatus>(filter, Column, mode)
            .Build();

        // Assert
        var expectedParam = "users_status_NotEq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.status <> @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<object>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));

        var convertedValue = Convert.ChangeType(paramDict[expectedParam], expectedType);
        Assert.Equal(expectedValue, convertedValue);
    }

    [Fact]
    public void BindEnum_InArray_Name_AddsIn()
    {
        // Arrange
        var filter = new EnumFilter<UserStatus>
        {
            In = [UserStatus.Active, UserStatus.Unknown]
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindEnum<UserStatus>(filter, Column, EnumFilterMode.Name)
            .Build();

        // Assert
        var expectedParam = "users_status_In_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.status = ANY(@{expectedParam})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<object[]>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(
            ["Active", "Unknown"],
            paramDict[expectedParam].Cast<string>().ToArray());
    }

    [Fact]
    public void BindEnum_InArray_Value_AddsIn()
    {
        // Arrange
        var filter = new EnumFilter<UserStatus>
        {
            In = [UserStatus.Active, UserStatus.Unknown]
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindEnum<UserStatus>(filter, Column, EnumFilterMode.Value)
            .Build();

        // Assert
        var expectedParam = "users_status_In_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.status = ANY(@{expectedParam})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<object[]>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(
            [1, 0],
            paramDict[expectedParam].Cast<int>().ToArray());
    }

    [Fact]
    public void BindEnum_InArray_ValueString_AddsIn()
    {
        // Arrange
        var filter = new EnumFilter<UserStatus>
        {
            In = [UserStatus.Active, UserStatus.Unknown]
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindEnum<UserStatus>(filter, Column, EnumFilterMode.ValueString)
            .Build();

        // Assert
        var expectedParam = "users_status_In_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.status = ANY(@{expectedParam})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<object[]>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(
            ["1", "0"],
            paramDict[expectedParam].Cast<string>().ToArray());
    }

    [Fact]
    public void BindEnum_NotInArray_Name_AddsNotIn()
    {
        // Arrange
        var filter = new EnumFilter<UserStatus>
        {
            NotIn = [UserStatus.Active, UserStatus.Unknown]
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindEnum<UserStatus>(filter, Column, EnumFilterMode.Name)
            .Build();

        // Assert
        var expectedParam = "users_status_NotIn_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.status <> ANY(@{expectedParam})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<object[]>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(
            ["Active", "Unknown"],
            paramDict[expectedParam].Cast<string>().ToArray());
    }

    [Fact]
    public void BindEnum_NotInArray_Value_AddsNotIn()
    {
        // Arrange
        var filter = new EnumFilter<UserStatus>
        {
            NotIn = [UserStatus.Active, UserStatus.Unknown]
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindEnum<UserStatus>(filter, Column, EnumFilterMode.Value)
            .Build();

        // Assert
        var expectedParam = "users_status_NotIn_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.status <> ANY(@{expectedParam})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<object[]>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(
            [1, 0],
            paramDict[expectedParam].Cast<int>().ToArray());
    }

    [Fact]
    public void BindEnum_NotInArray_ValueString_AddsNotIn()
    {
        // Arrange
        var filter = new EnumFilter<UserStatus>
        {
            NotIn = [UserStatus.Active, UserStatus.Unknown]
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindEnum<UserStatus>(filter, Column, EnumFilterMode.ValueString)
            .Build();

        // Assert
        var expectedParam = "users_status_NotIn_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.status <> ANY(@{expectedParam})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<object[]>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(
            ["1", "0"],
            paramDict[expectedParam].Cast<string>().ToArray());
    }

    [Fact]
    public void BindEnum_IsNullTrue_AddsIsNull()
    {
        // Arrange
        var filter = new EnumFilter<UserStatus> { IsNull = true };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindEnum<UserStatus>(filter, Column, EnumFilterMode.Name)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "WHERE users.status IS NULL",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindEnum_IsNullFalse_AddsIsNotNull()
    {
        // Arrange
        var filter = new EnumFilter<UserStatus> { IsNull = false };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindEnum<UserStatus>(filter, Column, EnumFilterMode.Name)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "WHERE users.status IS NOT NULL",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindEnum_WithSort_AddsOrderBy()
    {
        // Arrange
        var filter = new EnumFilter<UserStatus>
        {
            Sort = new Sort(position: 1, descending: true)
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindEnum<UserStatus>(filter, Column, EnumFilterMode.Name)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id FROM users ORDER BY users.status DESC",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Theory]
    [InlineData(EnumFilterMode.Name, "Active", typeof(string), "text")]
    [InlineData(EnumFilterMode.Value, 1, typeof(int), "int4")]
    [InlineData(EnumFilterMode.ValueString, "1", typeof(string), "text")]
    public void BindEnum_JsonbColumn_Eq_GeneratesCorrectCondition(
        EnumFilterMode mode,
        object expectedValue,
        Type expectedType,
        object sqlTypeCast)
    {
        // Arrange
        var filter = new EnumFilter<UserStatus>
        {
            Eq = UserStatus.Active,
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindEnum<UserStatus>(filter, "profile->>'status'", mode)
            .Build();

        // Assert
        var expectedParam = "users_profile_status_Eq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE (users.profile->>'status')::{sqlTypeCast} = @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<object>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));

        var convertedValue = Convert.ChangeType(paramDict[expectedParam], expectedType);
        Assert.Equal(expectedValue, convertedValue);
    }
}
