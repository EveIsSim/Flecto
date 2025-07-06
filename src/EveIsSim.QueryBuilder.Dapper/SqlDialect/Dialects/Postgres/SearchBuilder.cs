using EveIsSim.QueryBuilder.Core.Models.Filters;
using EveIsSim.QueryBuilder.Dapper.Commons;
using EveIsSim.QueryBuilder.Dapper.Constants;

namespace EveIsSim.QueryBuilder.Dapper.SqlDialect.Dialects.Postgres;

internal static class SearchBuilder
{
    internal static (string SqlCondition, string ParamValue) Build(
        SearchFilter filter,
        string paramName,
        (string Table, string[] Columns)[] tablesWithColumns)
    {
        var conditions = new List<string>();
        var op = CommonSqlBuilder.LikeOperator(filter.CaseSensitive);

        foreach (var (table, columns) in tablesWithColumns)
        {
            foreach (var column in columns)
            {
                var colRef = Common.CombineColumn(table, column);
                var condition = $"{colRef} {op} @{paramName}";

                conditions.Add(condition);
            }
        }

        var c = $"({string.Join($" {Sql.OR} ", conditions)})";
        var v = $"%{filter.Value}%";

        return (c, v);
    }

    internal static string BuildTsVectorSearchCondition(
        string paramName,
        TextSearchMode mode,
        string config,
        (string Table, string[] Columns)[] tablesWithColumns)
    {
        var columns = tablesWithColumns
            .SelectMany(tc => tc.Columns
                .Select(c => $"{Sql.COALESCE}({tc.Table}.{c}, '')"));

        var concatenatedColumns = string.Join(" || ' ' || ", columns);
        var tsvector = $"to_tsvector('{config}', {concatenatedColumns})";

        var tsquery = mode switch
        {
            TextSearchMode.Plain => $"plainto_tsquery('{config}', @{paramName})",
            TextSearchMode.WebStyle => $"websearch_to_tsquery('{config}', @{paramName})",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported search mode")
        };

        return $"{tsvector} @@ {tsquery}";
    }
}
