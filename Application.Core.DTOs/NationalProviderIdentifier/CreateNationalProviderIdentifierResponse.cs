namespace Application.Core.DTOs.NationalProviderIdentifier;

public sealed record CreateNationalProviderIdentifierResponse
{
    public required string NationalProviderIdentifier{ get; init; }
}