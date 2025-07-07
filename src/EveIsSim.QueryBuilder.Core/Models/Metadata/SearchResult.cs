namespace EveIsSim.QueryBuilder.Core.Models.Metadata;


public record SearchResult<T>(T Data, SearchMetadata Metadata);
