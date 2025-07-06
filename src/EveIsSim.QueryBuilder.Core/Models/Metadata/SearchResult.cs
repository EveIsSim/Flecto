namespace EveIsSim.QueryBuilder.Models.Metadata;


public record SearchResult<T>(T Data, SearchMetadata Metadata);
