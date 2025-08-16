using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Select;

namespace Flecto.Dapper.UnitTests.FlectoBuilderTests;

public class SortTests
{
    private const string Table = "users";
    private readonly FromTable _tc = new(Table, [new("id")]);

    [Fact]
    public void Sort_Single_AddSingleOrderBy()
    {
        // Arrange
        var filter = new BoolFilter
        {
            Sort = new Sort(position: 1, descending: true)
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindBool(filter, "is_active")
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "ORDER BY users.is_active DESC",
            result.Sql
        );

        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void Sort_MultiSorting_AddOrderByWithValidPositions()
    {
        // Arrange
        var filter1 = new BoolFilter
        {
            Sort = new Sort(position: 1, descending: true)
        };

        var filter2 = new NumericFilter<int>
        {
            Sort = new Sort(position: 2, descending: false)
        };

        var filter3 = new BoolFilter
        {
            Sort = new Sort(position: 3, descending: true)
        };

        var filter4 = new NumericFilter<int>
        {
            Sort = new Sort(position: 4, descending: true)
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindBool(filter1, "bool_pos_1")
            .BindNumeric(filter4, "int_pos_4")
            .BindBool(filter3, "profile->'bool_pos_3'")
            .BindNumeric(filter2, "int_pos_2")
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "ORDER BY " +
                "users.bool_pos_1 DESC, " +
                "users.int_pos_2 ASC, " +
                "(users.profile->'bool_pos_3')::boolean DESC, " +
                "users.int_pos_4 DESC",
            result.Sql
        );

        Assert.Empty(result.Parameters.ParameterNames);
    }

    [Fact]
    public void Sort_MultiSorting_WhereSomePositionsIsEquals_AddOrders()
    {
        // Arrange
        var filter1 = new BoolFilter
        {
            Sort = new Sort(position: 1, descending: true)
        };

        var filter2 = new NumericFilter<int>
        {
            Sort = new Sort(position: 2, descending: false)
        };

        var filter3 = new BoolFilter
        {
            Sort = new Sort(position: 3, descending: true)
        };

        var filter4 = new NumericFilter<int>
        {
            Sort = new Sort(position: 2, descending: true)
        };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .BindBool(filter1, "bool_pos_1")
            .BindNumeric(filter4, "int_pos_4")
            .BindBool(filter3, "profile->'bool_pos_3'")
            .BindNumeric(filter2, "int_pos_2")
            .Build();

        // Assert
        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            "ORDER BY " +
                "users.bool_pos_1 DESC, " +
                "users.int_pos_4 DESC, " +
                "users.int_pos_2 ASC, " +
                "(users.profile->'bool_pos_3')::boolean DESC",
            result.Sql
        );

        Assert.Empty(result.Parameters.ParameterNames);
    }
}
