namespace Flecto.Dapper.Commons;

/// <summary>
/// Provides common helper methods for SQL query construction.
/// </summary>
internal static class Common
{
    /// <summary>
    /// Generates a unique parameter name for search or filtering operations by combining a prefix with a counter.
    /// This ensures uniqueness of parameter names within dynamically constructed SQL queries.
    /// Example: <c>GenSearchParamName("p", 3) => "p3"</c>.
    /// </summary>
    /// <param name="prefix">The prefix for the parameter name.</param>
    /// <param name="counter">The numerical counter to append, ensuring uniqueness.</param>
    /// <returns>The generated unique parameter name.</returns>
    internal static string GenSearchParamName(string prefix, int counter)
    => prefix + counter;
}
