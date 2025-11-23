using BindSharp;
using Infrastructure.Core.DTOs.License;
using Infrastructure.Core.Models.License;

namespace Infrastructure.Core.Interfaces.License;

public interface ILicenseNumberRepository
{
    Task<Result<Unit, LicenseNumberError>> AddAsync(LicenseNumber licenseNumber);
}