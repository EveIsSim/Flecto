using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Select;

namespace Flecto.Dapper.UnitTests.FlectoBuilderTests;

public class BindFlagsEnumTests
{
    private const string Table = "users";
    private const string Column = "access";
    private readonly FromTable _tc = new FromTable(Table, new Field[] { new("id") });

    [Flags]
    private enum Access
    {
        Nont = 0,
        Read = 1 << 0, // 1
        Write = 1 << 1, // 2
        Admin = 1 << 2, // 4
    }

    [Fact]
    public void BindFlagsEnum_FilterIsNull_NotAddCondition()
    {
        // Arrange
        FlagsEnumFilter<Access>? filter = null;
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindFlagsEnum<Access>(filter, Column)
            .Build();

        // Assert
        Assert.Equal("SELECT users.id FROM users", result.Sql);
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindFlagsEnum_ValidationFailes_ThrowException()
    {
        // Arrange
        var filter = new FlagsEnumFilter<Access>
        {
            Eq = Access.Read,
            NotEq = Access.Write
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            builder
            .Select(_tc)
            .BindFlagsEnum<Access>(filter, Column)
            .Build());

        // Assert
        var expected = """
            FlagsEnumFilter`1: validation for table: 'users', column: 'access' failed:
            FlagsEnumFilter: Cannot specify both Eq and NotEq simultaneously
            """;
        Assert.Equal(expected, ex.Message);
    }

    [Fact]
    public void BindFlagsEnum_WithEq_AddsEqualCondition()
    {
        // Arrange
        var filter = new FlagsEnumFilter<Access>
        {
            Eq = Access.Read
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindFlagsEnum<Access>(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_access_Eq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.access = @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<long>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal((long)filter.Eq, paramDict[expectedParam]);
    }

    [Fact]
    public void BindFlagsEnum_WithNotEq_AddsNotEqualCondition()
    {
        // Arrange
        var filter = new FlagsEnumFilter<Access>
        {
            NotEq = Access.Read
        };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindFlagsEnum<Access>(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_access_NotEq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.access <> @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<long>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal((long)filter.NotEq, paramDict[expectedParam]);
    }

    [Fact]
    public void BindFlagsEnum_HasFlag_AddsBitwiseAndEqualFlag()
    {
        // Arrange
        var filter = new FlagsEnumFilter<Access>
        {
            HasFlag = Access.Read | Access.Write
        };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindFlagsEnum<Access>(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_access_HasFlag_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.access & @{expectedParam} <> 0",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<long>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(3, paramDict[expectedParam]);
    }

    [Fact]
    public void BindFlagsEnum_NotHasFlag_AddsBitwiseAndEqualZero()
    {
        // Arrange
        var filter = new FlagsEnumFilter<Access>
        {
            NotHasFlag = Access.Admin
        };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindFlagsEnum<Access>(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_access_NotHasFlag_0";

        Console.WriteLine(result.Sql);
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.access & @{expectedParam} = 0",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<long>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal((long)Access.Admin, paramDict[expectedParam]);
    }

    [Fact]
    public void BindFlagsEnum_IsNullTrue_AddsIsNull()
    {
        // Arrange
        var filter = new FlagsEnumFilter<Access> { IsNull = true };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindFlagsEnum<Access>(filter, Column)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "WHERE users.access IS NULL",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindFlagsEnum_IsNullFalse_AddsIsNull()
    {
        // Arrange
        var filter = new FlagsEnumFilter<Access> { IsNull = false };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindFlagsEnum<Access>(filter, Column)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "WHERE users.access IS NOT NULL",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindFlagsEnum_WithSort_AddsOrderBy()
    {
        // Arrange
        var filter = new FlagsEnumFilter<Access>
        {
            Sort = new Sort(position: 1, descending: true)
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindFlagsEnum<Access>(filter, Column)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id FROM users ORDER BY users.access DESC",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindFlagsEnum_JsonbColumn_Eq_GeneratesCorrectCondition()
    {
        // Arrange
        var filter = new FlagsEnumFilter<Access>
        {
            Eq = Access.Read
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindFlagsEnum<Access>(filter, "profile->>'access'")
            .Build();

        // Assert
        var expectedParam = "users_profile_access_Eq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE (users.profile->>'access')::int4 = @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<long>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal((long)filter.Eq, paramDict[expectedParam]);
    }

    [Fact]
    public void BindFlagsEnum_Jsonb_HasFlag_GeneratesCorrectCondition()
    {
        // Arrange
        var filter = new FlagsEnumFilter<Access>
        {
            HasFlag = Access.Read | Access.Admin
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder.Select(_tc)
            .BindFlagsEnum(filter, "profile->>'access'")
            .Build();

        // Assert
        var expectedParam = "users_profile_access_HasFlag_0";
        Console.WriteLine(result.Sql);
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE (users.profile->>'access')::int4 & @{expectedParam} <> 0",
            result.Sql
        );


        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<long>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(5, paramDict[expectedParam]);
    }

    [Fact]
    public void BindFlagsEnum_MultiBind_AddConditions()
    {
        // Arrange
        var filter0 = new FlagsEnumFilter<Access> { Eq = Access.Admin };
        var filter1 = new FlagsEnumFilter<Access> { Eq = Access.Read };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindFlagsEnum<Access>(filter0, Column)
            .BindFlagsEnum<Access>(filter1, Column)
            .Build();

        // Assert
        var expectedParam0 = "users_access_Eq_0";
        var expectedParam1 = "users_access_Eq_1";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.access = @{expectedParam0} " +
                $"AND users.access = @{expectedParam1}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<long>(name));

        Assert.Equal(2, paramDict.Count());
        Assert.True(paramDict.ContainsKey(expectedParam0));
        Assert.Equal((long)filter0.Eq, paramDict[expectedParam0]);

        Assert.True(paramDict.ContainsKey(expectedParam1));
        Assert.Equal((long)filter1.Eq, paramDict[expectedParam1]);
    }
}
