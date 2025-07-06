namespace EveIsSim.QueryBuilder.Core.Models.Filters;

public enum TextSearchMode
{
    Plain,      // plainto_tsquery
    WebStyle    // websearch_to_tsquery
}
