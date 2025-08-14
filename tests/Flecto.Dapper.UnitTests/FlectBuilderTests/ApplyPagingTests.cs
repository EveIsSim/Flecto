using Flecto.Core.Enums;
using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Select;

namespace Flecto.Dapper.UnitTests.FlectoBuilderTests;

public class ApplyPagingTests
{
    private const string Table = "users";
    private readonly FromTable _tc = new FromTable(Table, new Field[] { new("id") });

    [Fact]
    public void ApplyPaging_ValidationFailes_ThrowException()
    {
        // Arrange
        PaginationFilter? filter = null;
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            builder
            .Select(_tc)
            .ApplyPaging(filter!)
            .Build());

        // Assert
        var expected = """
            PaginationFilter: validation failed:
            PaginationFilter: PaginationFilter is required but was null
            """;
        Assert.Equal(expected, ex.Message);
    }

    [Fact]
    public void ApplyPaging_Success()
    {
        // Arrange
        var filter = new PaginationFilter { Limit = 10, Page = 5 };
        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .ApplyPaging(filter)
            .Build();

        // Assert
        var expectedLimitParam = "_Limit";
        var expectedOffsetParam = "_Offset";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"LIMIT @{expectedLimitParam} OFFSET @{expectedOffsetParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<int>(name));

        Assert.Equal(2, paramDict.Count());

        Assert.True(paramDict.ContainsKey(expectedLimitParam));
        Assert.Equal(filter.Limit, paramDict[expectedLimitParam]);

        Assert.True(paramDict.ContainsKey(expectedOffsetParam));
        Assert.Equal(40, paramDict[expectedOffsetParam]);

    }

    [Fact]
    public void ApplyPaging_MultipleApplying_ShouldApplyOnlyLast()
    {
        // Arrange
        var filter1 = new PaginationFilter { Limit = 10, Page = 1 };
        var filter2 = new PaginationFilter { Limit = 20, Page = 2 };
        var filter3 = new PaginationFilter { Limit = 30, Page = 3 };

        var builder = new FlectoBuilder(Table, DialectType.Postgres);

        // Act
        var result = builder
            .Select(_tc)
            .ApplyPaging(filter1)
            .ApplyPaging(filter2)
            .ApplyPaging(filter3)
            .Build();

        // Assert
        var expectedLimitParam = "_Limit";
        var expectedOffsetParam = "_Offset";

        Assert.Equal(
            "SELECT users.id " +
            "FROM users " +
            $"LIMIT @{expectedLimitParam} OFFSET @{expectedOffsetParam}",
            result.Sql
        );

        var paramDict = result.Parameters.ParameterNames
            .ToDictionary(
                name => name,
                name => result.Parameters.Get<int>(name));

        Assert.Equal(2, paramDict.Count());

        Assert.True(paramDict.ContainsKey(expectedLimitParam));
        Assert.Equal(filter3.Limit, paramDict[expectedLimitParam]);

        Assert.True(paramDict.ContainsKey(expectedOffsetParam));
        Assert.Equal(60, paramDict[expectedOffsetParam]);

    }
}
