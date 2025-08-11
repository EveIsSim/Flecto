using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Select;

namespace Flecto.Dapper.UnitTests.FlectoBuilderTests;

public class BindNumericTests
{
    private const string Table = "users";
    private const string Column = "value";
    private readonly FromTable _tc = new FromTable(Table, new Field[] { new("id") });

    [Fact]
    public void BindNumeric_FilterIsNull_NotAddCondition()
    {
        // Arrange
        NumericFilter<int>? filter = null;
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric<int>(filter, Column)
            .Build();

        // Assert
        Assert.Equal("SELECT users.id FROM users", result.Sql);
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindNumeric_ValidationFailes_ThrowException()
    {
        // Arrange
        var filter = new NumericFilter<int>
        {
            Eq = 10,
            NotEq = 20
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            builder
            .Select(_tc)
            .BindNumeric<int>(filter, Column)
            .Build());

        // Assert
        var expected = """
            NumericFilter`1: validation for table: 'users', column: 'value' failed:
            NumericFilter: Cannot specify both Eq and NotEq simultaneously
            """;
        Assert.Equal(expected, ex.Message);
    }

    [Fact]
    public void BindNumeric_UnsupportedGenericType_Throws()
    {
        // Arrange
        var filter = new NumericFilter<byte> { Eq = 1 };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            builder
            .Select(_tc)
            .BindNumeric<byte>(filter, Column)
            .Build());

        // Assert
        var expected = """
            NumericFilter<Byte> is not supported for column 'value'. (Parameter 'filter')
            """;
        Assert.Equal(expected, ex.Message);
    }

    [Fact]
    public void BindNumeric_WithEq_AddsEqualCondition()
    {
        // Arrange
        var filter = new NumericFilter<int>
        {
            Eq = 10
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_value_Eq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.value = @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<int>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.Eq, paramDict[expectedParam]);
    }

    [Fact]
    public void BindNumeric_WithNotEq_AddsNotEqualCondition()
    {
        // Arrange
        var filter = new NumericFilter<int>
        {
            NotEq = 10
        };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_value_NotEq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.value <> @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<int>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.NotEq, paramDict[expectedParam]);
    }

    [Fact]
    public void BindNumeric_WithRange_GteAndLt_AddsTwoConditions()
    {
        // Arrange
        var filter = new NumericFilter<int>
        {
            Gte = 10,
            Lt = 20
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter, Column)
            .Build();

        // Assert
        var expectedGte = "users_value_Gte_0";
        var expectedLt = "users_value_Lt_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.value >= @{expectedGte} " +
                $"AND users.value < @{expectedLt}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<int>(name));

        Assert.Equal(2, paramDict.Count);
        Assert.Equal(filter.Gte, paramDict[expectedGte]);
        Assert.Equal(filter.Lt, paramDict[expectedLt]);
    }

    [Fact]
    public void BindNumeric_WithRange_All_AddsFourConditions()
    {
        // Arrange
        // |--Gt--Gte--Lte--Lt--|
        var filter = new NumericFilter<int>
        {
            Gt = 10,
            Gte = 20,
            Lte = 30,
            Lt = 40,
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter, Column)
            .Build();

        // Assert
        var expectedGt = "users_value_Gt_0";
        var expectedGte = "users_value_Gte_0";
        var expectedLte = "users_value_Lte_0";
        var expectedLt = "users_value_Lt_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.value > @{expectedGt} " +
                $"AND users.value >= @{expectedGte} " +
                $"AND users.value < @{expectedLt} " +
                $"AND users.value <= @{expectedLte}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<int>(name));

        Assert.Equal(4, paramDict.Count);
        Assert.Equal(filter.Gt, paramDict[expectedGt]);
        Assert.Equal(filter.Gte, paramDict[expectedGte]);
        Assert.Equal(filter.Lte, paramDict[expectedLte]);
        Assert.Equal(filter.Lt, paramDict[expectedLt]);
    }

    [Fact]
    public void BindNumeric_InArray_AddsIn()
    {
        // Arrange
        var filter = new NumericFilter<int>
        {
            In = [
                10, 20
            ]
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter, Column)
            .Build();

        // Assert
        var expectedIn = "users_value_In_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.value = ANY(@{expectedIn})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<int[]>(name));

        Assert.Single(paramDict);
        Assert.Equal(filter.In, paramDict[expectedIn]);
    }

    [Fact]
    public void BindNumeric_NotInArray_AddsNotIn()
    {
        // Arrange
        var filter = new NumericFilter<int>
        {
            NotIn = [
                10, 20
            ]
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter, Column)
            .Build();

        // Assert
        var expectedIn = "users_value_NotIn_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.value <> ANY(@{expectedIn})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<int[]>(name));

        Assert.Single(paramDict);
        Assert.Equal(filter.NotIn, paramDict[expectedIn]);
    }

    [Fact]
    public void BindNumeric_IsNullTrue_AddsIsNull()
    {
        // Arrange
        var filter = new NumericFilter<int> { IsNull = true };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter, Column)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "WHERE users.value IS NULL",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindNumeric_IsNullFalse_AddsIsNotNull()
    {
        // Arrange
        var filter = new NumericFilter<int> { IsNull = false };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter, Column)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "WHERE users.value IS NOT NULL",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindNumeric_WithSort_AddsOrderBy()
    {
        // Arrange
        var filter = new NumericFilter<int>
        {
            Sort = new Sort(position: 1, descending: true)
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter, Column)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id FROM users ORDER BY users.value DESC",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindNumeric_MultiBind_AddConditions()
    {
        // Arrange
        var filter0 = new NumericFilter<int> { Eq = 10 };
        var filter1 = new NumericFilter<int> { Eq = 20 };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter0, Column)
            .BindNumeric(filter1, Column)
            .Build();

        // Assert
        var expectedParam0 = "users_value_Eq_0";
        var expectedParam1 = "users_value_Eq_1";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.value = @{expectedParam0} " +
                $"AND users.value = @{expectedParam1}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<int>(name));

        Assert.Equal(2, paramDict.Count());
        Assert.True(paramDict.ContainsKey(expectedParam0));
        Assert.Equal(filter0.Eq, paramDict[expectedParam0]);

        Assert.True(paramDict.ContainsKey(expectedParam1));
        Assert.Equal(filter1.Eq, paramDict[expectedParam1]);
    }

    [Fact]
    public void BindNumeric_Short_Jsonb_CastsToInt2()
    {
        // Arrange
        var filter = new NumericFilter<short> { Gt = 10 };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter, "profile->>'value'")
            .Build();

        // Assert
        var expectedParam = "users_profile_value_Gt_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE (users.profile->>'value')::int2 > @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<short>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.Gt, paramDict[expectedParam]);
    }

    [Fact]
    public void BindNumeric_Int_Jsonb_CastsToInt4()
    {
        // Arrange
        var filter = new NumericFilter<int> { Gt = 10 };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter, "profile->>'value'")
            .Build();

        // Assert
        var expectedParam = "users_profile_value_Gt_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE (users.profile->>'value')::int4 > @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<int>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.Gt, paramDict[expectedParam]);
    }

    [Fact]
    public void BindNumeric_Long_Jsonb_CastsToInt8()
    {
        // Arrange
        var filter = new NumericFilter<long> { Gt = 10L };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter, "profile->>'value'")
            .Build();

        // Assert
        var expectedParam = "users_profile_value_Gt_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE (users.profile->>'value')::int8 > @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<long>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.Gt, paramDict[expectedParam]);
    }

    [Fact]
    public void BindNumeric_Float_Jsonb_CastsToFloat4()
    {
        // Arrange
        var filter = new NumericFilter<float> { Gt = 3.14f };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter, "profile->>'value'")
            .Build();

        // Assert
        var expectedParam = "users_profile_value_Gt_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE (users.profile->>'value')::float4 > @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<float>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.Gt, paramDict[expectedParam]);
    }

    [Fact]
    public void BindNumeric_Double_Jsonb_CastsToFloat8()
    {
        // Arrange
        var filter = new NumericFilter<double> { Gt = 123.456 };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter, "profile->>'value'")
            .Build();

        // Assert
        var expectedParam = "users_profile_value_Gt_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE (users.profile->>'value')::float8 > @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<double>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.Gt, paramDict[expectedParam]);
    }

    [Fact]
    public void BindNumeric_Decimal_Jsonb_CastsToNumeric()
    {
        // Arrange
        var filter = new NumericFilter<decimal> { Gt = 9999.99m };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindNumeric(filter, "profile->>'value'")
            .Build();

        // Assert
        var expectedParam = "users_profile_value_Gt_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE (users.profile->>'value')::numeric > @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<decimal>(name));

        Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.Gt, paramDict[expectedParam]);
    }
}

