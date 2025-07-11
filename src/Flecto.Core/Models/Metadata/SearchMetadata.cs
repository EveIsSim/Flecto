using Flecto.Core.Models.Filters;

namespace Flecto.Core.Models.Metadata;

/// <summary>
/// Represents user-friendly pagination metadata for a search query
/// </summary>
/// <param name="Page">The current page number of the query results</param>
/// <param name="Limit">The maximum number of items per page</param>
/// <param name="TotalRecords">The total number of records available</param>
/// <param name="TotalPages">The total number of pages based on the total records and page size</param>
public record SearchMetadata(int Page, int Limit, int TotalRecords, int TotalPages)
{
    /// <summary>
    /// Creates a new <see cref="SearchMetadata"/> instance from the total number of records and the specified pagination filter
    /// </summary>
    /// <param name="totalRecords">The total number of records available</param>
    /// <param name="paginationFilter">The pagination filter containing the page and limit information.</param>
    /// <returns>A <see cref="SearchMetadata"/> instance with calculated total pages.</returns>
    public static SearchMetadata From(int totalRecords, PaginationFilter paginationFilter)
    => new SearchMetadata(
        paginationFilter.Page,
        paginationFilter.Limit,
        totalRecords,
        (totalRecords + paginationFilter.Limit - 1) / paginationFilter.Limit);
}
