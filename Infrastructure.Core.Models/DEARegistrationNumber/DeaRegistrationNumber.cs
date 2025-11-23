namespace Infrastructure.Core.Models.DEARegistrationNumber;

public sealed record DeaRegistrationNumber
{
    public required Guid DeaRegistrationNumberId { get; init; }
    public required string DeaRegistrationNumberValue { get; init; }
    public required DateTime CreatedAt { get; init; }
}