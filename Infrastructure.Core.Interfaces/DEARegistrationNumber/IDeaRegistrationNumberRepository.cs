using BindSharp;
using Infrastructure.Core.DTOs.DEARegistrationNumber;
using Infrastructure.Core.Models.DEARegistrationNumber;

namespace Infrastructure.Core.Interfaces.DEARegistrationNumber;

public interface IDeaRegistrationNumberRepository
{
    Task<Result<Unit, DeaRegistrationNumberError>> AddAsync(DeaRegistrationNumber deaNumber);
}