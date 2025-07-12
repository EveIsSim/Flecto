using Flecto.Core.Models.Filters;
using Flecto.Core.Models.Metadata;

namespace Flecto.Core.UnitTests.SearchMetadataTests;

public class FromTests
{
    [Fact]
    public void From_ShouldSuccess()
    {
        // Arrange
        var total = 100;
        var filter = new PaginationFilter { Limit = 20, Page = 3 };

        // Act
        var result = SearchMetadata.From(total, filter);

        // Assert
        Assert.Equal(filter.Page, result.Page);
        Assert.Equal(filter.Limit, result.Limit);
        Assert.Equal(total, result.TotalRecords);
        Assert.Equal(5, result.TotalPages);
    }

    [Fact]
    public void From_TotalRecords_EqZero_ShouldTotalZero()
    {
        // Arrange
        var total = 0;
        var filter = new PaginationFilter { Limit = 10, Page = 3 };

        // Act
        var result = SearchMetadata.From(total, filter);

        // Assert
        Assert.Equal(filter.Page, result.Page);
        Assert.Equal(filter.Limit, result.Limit);
        Assert.Equal(total, result.TotalRecords);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void From_TotalRecords_LtZero_ShouldTotalZero()
    {
        // Arrange
        var total = -1;
        var filter = new PaginationFilter { Limit = 10, Page = 3 };

        // Act
        var result = SearchMetadata.From(total, filter);

        // Assert
        Assert.Equal(filter.Page, result.Page);
        Assert.Equal(filter.Limit, result.Limit);
        Assert.Equal(total, result.TotalRecords);
        Assert.Equal(0, result.TotalPages);
    }

}
