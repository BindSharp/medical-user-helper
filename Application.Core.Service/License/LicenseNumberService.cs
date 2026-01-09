using Application.Core.DTOs.License;
using Application.Core.Interfaces.License;
using BindSharp;
using Infrastructure.Core.DTOs.License;
using Infrastructure.Core.Interfaces.License;
using Infrastructure.Core.Models.License;

namespace Application.Core.Service.License;

public sealed class LicenseNumberService : BaseService, ILicenseNumber
{
    private readonly ILicenseNumberRepository _licenseNumberRepository;
    
    public LicenseNumberService(ILicenseNumberRepository licenseNumberRepository)
    {
        _licenseNumberRepository = licenseNumberRepository;
    }

    public async Task<Result<CreateLicenseNumberResponse, LicenseNumberError>> CreateLicenseNumber(string stateCode, string lastName, LicenseNumberType licenseNumberType)
    {
        var licenseNumber = new LicenseNumber
        {
            LicenseNumberId = Guid.CreateVersion7(),
            LicenseNumberValue = GenerateLicenseNumber(stateCode, lastName, licenseNumberType),
            CreatedAt = DateTime.UtcNow
        };
        
        return await IsStringEmpty<LicenseNumberNoStateCodeError>(stateCode)
            .MapError(LicenseNumberError (error) => error)
            .Bind(_ => IsStringEmpty<LicenseNumberNoLastNameError>(lastName)
                .MapError(LicenseNumberError (error) => error))
            .BindAsync(async _ => await _licenseNumberRepository.AddAsync(licenseNumber))
            .MapAsync(_ => new CreateLicenseNumberResponse
            {
                LicenseNumber = licenseNumber.LicenseNumberValue
            });
    }
    
    private static string GenerateLicenseNumber(string stateCode, string lastName, LicenseNumberType licenseNumberType) =>
        licenseNumberType switch
        {
            LicenseNumberType.Medical => GenerateMedicalLicense(stateCode, lastName),
            LicenseNumberType.Pharmacy => GeneratePharmacyLicense(stateCode, lastName),
            _ => GenerateMedicalLicense(stateCode, lastName)
        };

    private static string GenerateMedicalLicense(string stateCode, string lastName)
    {
        char lastNameInitial = char.ToUpper(lastName[0]);
    
        return stateCode switch
        {
            "CA" => GenerateLicenseWithChecksum("A", 5, lastNameInitial),
            "NY" => GenerateLicenseWithChecksum("", 6, lastNameInitial),
            "TX" => GenerateLicenseWithChecksum("", 5, lastNameInitial),
            "FL" => GenerateLicenseWithChecksum("ME", 5, lastNameInitial),
            _ => GenerateLicenseWithChecksum(stateCode, 6, lastNameInitial)
        };
    }

    private static string GeneratePharmacyLicense(string stateCode, string lastName)
    {
        char lastNameInitial = char.ToUpper(lastName[0]);
    
        return stateCode switch
        {
            "CA" => GenerateLicenseWithChecksum("RPH", 5, lastNameInitial),
            "NY" => GenerateLicenseWithChecksum("", 6, lastNameInitial),
            "TX" => GenerateLicenseWithChecksum("P", 5, lastNameInitial),
            "FL" => GenerateLicenseWithChecksum("PH", 5, lastNameInitial),
            _ => GenerateLicenseWithChecksum($"{stateCode}PH", 6, lastNameInitial)
        };
    }
    
    private static string GenerateLicenseWithChecksum(string prefix, int digitCount, char initial)
    {
        string digits = Random.Shared.Next((int)Math.Pow(10, digitCount - 1), (int)Math.Pow(10, digitCount)).ToString();
        int checksum = CalculateLicenseChecksum(digits);
    
        int totalLength = prefix.Length + 1 + digitCount + 1;
    
        return string.Create(totalLength, (prefix, initial, digits, checksum), 
            (span, state) =>
            {
                int pos = 0;
            
                if (state.prefix.Length > 0)
                {
                    state.prefix.AsSpan().CopyTo(span);
                    pos += state.prefix.Length;
                }
            
                span[pos++] = state.initial;
            
                state.digits.AsSpan().CopyTo(span.Slice(pos, state.digits.Length));
                pos += state.digits.Length;
            
                span[pos] = (char)('0' + state.checksum);
            });
    }
    
    private static int CalculateLicenseChecksum(string digits)
    {
        int sum = 0;
        for (int i = 0; i < digits.Length; i++)
        {
            int digit = digits[i] - '0';
            sum += (i % 2 == 0) ? digit * 2 : digit;
        }
        return sum % 10;
    }
}