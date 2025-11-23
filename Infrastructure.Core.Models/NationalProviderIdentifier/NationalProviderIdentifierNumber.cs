namespace Infrastructure.Core.Models.NationalProviderIdentifier;

public sealed record NationalProviderIdentifierNumber
{
    public required Guid NationalProviderIdentifierNumberId { get; init; }
    public required string NationalProviderIdentifier { get; init; }
    public required DateTime CreatedAt { get; init; }
}