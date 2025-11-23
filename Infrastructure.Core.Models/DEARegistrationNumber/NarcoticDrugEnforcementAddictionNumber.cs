namespace Infrastructure.Core.Models.DEARegistrationNumber;

public record NarcoticDrugEnforcementAddictionNumber
{
    public required Guid NarcoticDrugEnforcementAddictionNumberId { get; init; }
    public required string NarcoticDrugEnforcementAddictionNumberValue { get; init; }
    public required DateTime CreatedAt { get; init; }
}