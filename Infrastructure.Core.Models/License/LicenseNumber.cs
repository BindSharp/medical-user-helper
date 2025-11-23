namespace Infrastructure.Core.Models.License;

public sealed record LicenseNumber
{
    public required Guid LicenseNumberId { get; init; }
    public required string LicenseNumberValue { get; init; }
    public required DateTime CreatedAt { get; init; }
}