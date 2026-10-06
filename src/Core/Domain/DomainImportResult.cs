namespace Core.Domain;

public sealed record DomainImportResult<T>(
    IReadOnlyList<T> Entities,
    IReadOnlyList<string> Errors);
