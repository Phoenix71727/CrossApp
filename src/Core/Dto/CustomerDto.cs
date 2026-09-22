namespace Core.Dto;

public record CustomerDto(
    string Id,
    string FullName,
    string Email,
    string? Phone = null) : IEntityDto;
