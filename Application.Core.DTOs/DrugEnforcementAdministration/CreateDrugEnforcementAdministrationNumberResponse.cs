namespace Application.Core.DTOs.DrugEnforcementAdministration;

public sealed record CreateDrugEnforcementAdministrationNumberResponse
{
    public required string DrugEnforcementAdministrationNumber{ get; init; }
}