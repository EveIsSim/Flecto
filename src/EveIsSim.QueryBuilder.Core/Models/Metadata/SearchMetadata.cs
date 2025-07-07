using EveIsSim.QueryBuilder.Core.Models.Filters;

namespace EveIsSim.QueryBuilder.Core.Models.Metadata;


public record SearchMetadata(int Page, int Limit, int TotalRecords, int TotalPages)
{
    public static SearchMetadata From(int totalRecords, PaginationFilter paginationFilter)
    => new SearchMetadata(
        paginationFilter.Page,
        paginationFilter.Limit,
        totalRecords,
        (totalRecords + paginationFilter.Limit - 1) / paginationFilter.Limit);
}
