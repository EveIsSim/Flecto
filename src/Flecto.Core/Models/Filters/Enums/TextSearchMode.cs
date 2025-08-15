namespace Flecto.Core.Models.Filters.Enums;

/// <summary>
/// Specifies the text search mode used for full-text search operations
/// </summary>
public enum TextSearchMode
{
    /// <summary>
    /// Uses the plain text search mode (<c>plainto_tsquery</c>)
    /// </summary>
    Plain,
    /// <summary>
    /// Uses the web-style text search mode (<c>websearch_to_tsquery</c>)
    /// </summary>
    WebStyle
}
