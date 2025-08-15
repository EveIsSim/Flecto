using System.Globalization;
using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Select;

namespace Flecto.Dapper.UnitTests.FlectoBuilderTests;

public class BindDateTests
{
    private const string Table = "users";
    private const string Column = "created_at";
    private readonly FromTable _tc = new(Table, [new("id")]);

    [Fact]
    public void BindDate_FilterIsNull_NotAddCondition()
    {
        // Arrange
        DateFilter? filter = null;
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindDate(filter, Column)
            .Build();

        // Assert
        Assert.Equal("SELECT users.id FROM users", result.Sql);
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindDate_ValidationFailes_ThrowException()
    {
        // Arrange
        var filter = new DateFilter
        {
            Eq = new DateTime(2025, 08, 08),
            NotEq = new DateTime(2025, 09, 09)
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            builder
            .Select(_tc)
            .BindDate(filter, Column)
            .Build());

        // Assert
        var expected = """
            DateFilter: validation for table: 'users', column: 'created_at' failed:
            DateFilter: Cannot specify both Eq and NotEq simultaneously
            """;
        Assert.Equal(expected, ex.Message);
    }

    [Fact]
    public void BindDate_WithEq_AddsEqualCondition()
    {
        // Arrange
        var filter = new DateFilter
        {
            Eq = DateTime.Parse("2021-05-10T00:00:00Z", CultureInfo.InvariantCulture)
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindDate(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_created_at_Eq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.created_at = @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                static name => name,
                result.Parameters.Get<DateTime>);

        _ = Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.Eq, paramDict[expectedParam]);
    }

    [Fact]
    public void BindDate_WithNotEq_AddsNotEqualCondition()
    {
        // Arrange
        var filter = new DateFilter
        {
            NotEq = DateTime.Parse("2025-08-08T00:00:00Z", CultureInfo.InvariantCulture)
        };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindDate(filter, Column)
            .Build();

        // Assert
        var expectedParam = "users_created_at_NotEq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.created_at <> @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                result.Parameters.Get<DateTime>);

        _ = Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.NotEq, paramDict[expectedParam]);
    }

    [Fact]
    public void BindDate_WithRange_GteAndLt_AddsTwoConditions()
    {
        // Arrange
        var filter = new DateFilter
        {
            Gte = DateTime.Parse("2025-01-01T00:00:00Z", CultureInfo.InvariantCulture),
            Lt = DateTime.Parse("2025-08-08T00:00:00Z", CultureInfo.InvariantCulture)
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindDate(filter, Column)
            .Build();

        // Assert
        var expectedGte = "users_created_at_Gte_0";
        var expectedLt = "users_created_at_Lt_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.created_at >= @{expectedGte} " +
                $"AND users.created_at < @{expectedLt}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                result.Parameters.Get<DateTime>);

        Assert.Equal(2, paramDict.Count);
        Assert.Equal(filter.Gte, paramDict[expectedGte]);
        Assert.Equal(filter.Lt, paramDict[expectedLt]);
    }

    [Fact]
    public void BindDate_WithRange_All_AddsFourConditions()
    {
        // Arrange
        // |--Gt--Gte--Lte--Lt--|
        var filter = new DateFilter
        {
            Gt = DateTime.Parse("2025-01-01T00:00:00Z", CultureInfo.InvariantCulture),
            Gte = DateTime.Parse("2025-01-02T00:00:00Z", CultureInfo.InvariantCulture),
            Lte = DateTime.Parse("2025-08-07T00:00:00Z", CultureInfo.InvariantCulture),
            Lt = DateTime.Parse("2025-08-08T00:00:00Z", CultureInfo.InvariantCulture),
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindDate(filter, Column)
            .Build();

        // Assert
        var expectedGt = "users_created_at_Gt_0";
        var expectedGte = "users_created_at_Gte_0";
        var expectedLte = "users_created_at_Lte_0";
        var expectedLt = "users_created_at_Lt_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.created_at > @{expectedGt} " +
                $"AND users.created_at >= @{expectedGte} " +
                $"AND users.created_at < @{expectedLt} " +
                $"AND users.created_at <= @{expectedLte}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                result.Parameters.Get<DateTime>);

        Assert.Equal(4, paramDict.Count);
        Assert.Equal(filter.Gt, paramDict[expectedGt]);
        Assert.Equal(filter.Gte, paramDict[expectedGte]);
        Assert.Equal(filter.Lte, paramDict[expectedLte]);
        Assert.Equal(filter.Lt, paramDict[expectedLt]);
    }

    [Fact]
    public void BindDate_InArray_AddsIn()
    {
        // Arrange
        var filter = new DateFilter
        {
            In = [
                DateTime.Parse("2025-01-01T00:00:00Z", CultureInfo.InvariantCulture),
                DateTime.Parse("2025-08-08T00:00:00Z", CultureInfo.InvariantCulture),
            ]
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindDate(filter, Column)
            .Build();

        // Assert
        var expectedIn = "users_created_at_In_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.created_at = ANY(@{expectedIn})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<DateTime[]>(name));

        _ = Assert.Single(paramDict);
        Assert.Equal(filter.In, paramDict[expectedIn]);
    }

    [Fact]
    public void BindDate_NotInArray_AddsNotIn()
    {
        // Arrange
        var filter = new DateFilter
        {
            NotIn = [
                DateTime.Parse("2025-01-01T00:00:00Z", CultureInfo.InvariantCulture),
                DateTime.Parse("2025-08-08T00:00:00Z", CultureInfo.InvariantCulture),
            ]
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindDate(filter, Column)
            .Build();

        // Assert
        var expectedIn = "users_created_at_NotIn_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.created_at <> ANY(@{expectedIn})",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<DateTime[]>(name));

        _ = Assert.Single(paramDict);
        Assert.Equal(filter.NotIn, paramDict[expectedIn]);
    }

    [Fact]
    public void BindDate_IsNullTrue_AddsIsNull()
    {
        // Arrange
        var filter = new DateFilter { IsNull = true };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindDate(filter, Column)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "WHERE users.created_at IS NULL",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindDate_IsNullFalse_AddsIsNotNull()
    {
        // Arrange
        var filter = new DateFilter { IsNull = false };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindDate(filter, Column)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "WHERE users.created_at IS NOT NULL",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindDate_WithSort_AddsOrderBy()
    {
        // Arrange
        var filter = new DateFilter
        {
            Sort = new Sort(position: 1, descending: true)
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindDate(filter, Column)
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id FROM users ORDER BY users.created_at DESC",
            result.Sql
        );
        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void BindDate_JsonbColumn_Eq_GeneratesCorrectCondition()
    {
        // Arrange
        var filter = new DateFilter
        {
            Eq = DateTime.Parse("2025-01-01T00:00:00Z", CultureInfo.InvariantCulture),
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindDate(filter, "profile->>'created_at'")
            .Build();

        // Assert
        var expectedParam = "users_profile_created_at_Eq_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE (users.profile->>'created_at')::timestamptz = @{expectedParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                result.Parameters.Get<DateTime>);

        _ = Assert.Single(paramDict);
        Assert.True(paramDict.ContainsKey(expectedParam));
        Assert.Equal(filter.Eq, paramDict[expectedParam]);
    }

    [Fact]
    public void BindDate_MultiBind_AddConditions()
    {
        // Arrange
        var filter0 = new DateFilter { Eq = DateTime.Parse("2025-01-01T00:00:00Z", CultureInfo.InvariantCulture) };
        var filter1 = new DateFilter { Eq = DateTime.Parse("2025-08-08T00:00:00Z", CultureInfo.InvariantCulture) };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindDate(filter0, Column)
            .BindDate(filter1, Column)
            .Build();

        // Assert
        var expectedParam0 = "users_created_at_Eq_0";
        var expectedParam1 = "users_created_at_Eq_1";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.created_at = @{expectedParam0} " +
                $"AND users.created_at = @{expectedParam1}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                result.Parameters.Get<DateTime>);

        Assert.Equal(2, paramDict.Count);
        Assert.True(paramDict.ContainsKey(expectedParam0));
        Assert.Equal(filter0.Eq, paramDict[expectedParam0]);

        Assert.True(paramDict.ContainsKey(expectedParam1));
        Assert.Equal(filter1.Eq, paramDict[expectedParam1]);
    }

    [Fact]
    public void BindDate_MultiConditions()
    {
        // Arrange
        var filter = new DateFilter
        {
            NotIn = [
                DateTime.Parse("2020-05-01T00:00:00Z", CultureInfo.InvariantCulture),
                DateTime.Parse("2020-06-02T00:00:00Z", CultureInfo.InvariantCulture)
            ],
            In = [
                DateTime.Parse("2020-01-01T00:00:00Z", CultureInfo.InvariantCulture),
                DateTime.Parse("2020-02-02T00:00:00Z", CultureInfo.InvariantCulture)
            ],
            Gte = DateTime.Parse("2010-01-01T00:00:00Z", CultureInfo.InvariantCulture),
            Lt = DateTime.Parse("2030-01-01T00:00:00Z", CultureInfo.InvariantCulture),
            IsNull = false,
            NotEq = DateTime.Parse("2025-01-01T00:00:00Z", CultureInfo.InvariantCulture),
            Sort = new Sort(position: 1, descending: true)
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindDate(filter, Column)
            .Build();

        // Assert
        var notEqParam = "users_created_at_NotEq_0";
        var gteParam = "users_created_at_Gte_0";
        var ltParam = "users_created_at_Lt_0";
        var inParam = "users_created_at_In_0";
        var notInParam = "users_created_at_NotIn_0";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"WHERE users.created_at <> @{notEqParam} " +
                $"AND users.created_at >= @{gteParam} " +
                $"AND users.created_at < @{ltParam} " +
                $"AND users.created_at = ANY(@{inParam}) " +
                $"AND users.created_at <> ANY(@{notInParam}) " +
                "AND users.created_at IS NOT NULL " +
            "ORDER BY users.created_at DESC",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                static x => x,
                result.Parameters.Get<object?>);

        Assert.Equal(5, paramDict.Count);
        Assert.Equal(filter.NotEq, paramDict[notEqParam]);
        Assert.Equal(filter.Gte, paramDict[gteParam]);
        Assert.Equal(filter.Lt, paramDict[ltParam]);
        Assert.Equal(filter.In, paramDict[inParam]);
        Assert.Equal(filter.NotIn, paramDict[notInParam]);
    }
}
