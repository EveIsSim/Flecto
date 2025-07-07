namespace EveIsSim.QueryBuilder.Core.Models.Metadata;

/// <summary>
/// Represents a search result with data and associated pagination metadata
/// </summary>
/// <typeparam name="T">The type of the data returned by the search</typeparam>
/// <param name="Data">The data returned by the search</param>
/// <param name="Metadata">The pagination metadata associated with the search result</param>
public record SearchResult<T>(T Data, SearchMetadata Metadata);
