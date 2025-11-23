using Application.Core.DTOs.License;
using BindSharp;
using Infrastructure.Core.DTOs.License;

namespace Application.Core.Interfaces.License;

public interface ILicenseNumber
{
    Task<Result<CreateLicenseNumberResponse, LicenseNumberError>> CreateLicenseNumber(string stateCode, string lastName,
        LicenseNumberType licenseNumberType);
}