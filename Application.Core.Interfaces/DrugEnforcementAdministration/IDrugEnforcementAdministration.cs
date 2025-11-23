using Application.Core.DTOs.DrugEnforcementAdministration;
using BindSharp;
using Infrastructure.Core.DTOs.DEARegistrationNumber;

namespace Application.Core.Interfaces.DrugEnforcementAdministration;

public interface IDrugEnforcementAdministration
{
    Task<Result<CreateDrugEnforcementAdministrationNumberResponse, DeaRegistrationNumberError>>
        CreateDrugEnforcementAdministrationNumber(string lastName);

    Task<Result<NarcoticDrugEnforcementAddictionNumberResponse, NdeaRegistrationNumberError>>
        CreateNarcoticDrugEnforcementAddictionNumber(string lastName);
}