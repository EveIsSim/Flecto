namespace EveIsSim.QueryBuilder.Dapper.Commons;

internal static class Common
{
    internal static string CombineColumn(string table, string column)
    => table + "." + column;

    internal static string GenSearchParamName(string prefix, int counter)
    => prefix + counter;
}
