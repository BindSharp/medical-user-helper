using BindSharp;
using Infrastructure.Core.DTOs.DEARegistrationNumber;
using Infrastructure.Core.Models.DEARegistrationNumber;

namespace Infrastructure.Core.Interfaces.DEARegistrationNumber;

public interface INdeaRegistrationNumberRepository
{
    Task<Result<Unit, NdeaRegistrationNumberError>> AddAsync(NarcoticDrugEnforcementAddictionNumber ndeaNumber);
}