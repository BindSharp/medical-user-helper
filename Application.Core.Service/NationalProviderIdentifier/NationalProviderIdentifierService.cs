using System.Text;
using Application.Core.DTOs.NationalProviderIdentifier;
using Application.Core.Interfaces.NationalProviderIdentifier;
using BindSharp;
using Infrastructure.Core.DTOs.NationalProviderIdentifier;
using Infrastructure.Core.Interfaces.NationalProviderIdentifier;
using Infrastructure.Core.Models.NationalProviderIdentifier;

namespace Application.Core.Service.NationalProviderIdentifier;

public sealed class NationalProviderIdentifierService : INationalProviderIdentifier
{
    private readonly INationalProviderIdentifierRepository _nationalProviderIdentifierRepository;
    private const string LuhnCheckConstant = "80840";

    public NationalProviderIdentifierService(INationalProviderIdentifierRepository nationalProviderIdentifierRepository)
    {
        _nationalProviderIdentifierRepository = nationalProviderIdentifierRepository;
    }

    public async Task<Result<CreateNationalProviderIdentifierResponse, NationalProviderIdentifierError>>
        CreateNationalProviderIdentifier(bool isOrganization)
    {
        var npiNumber = new NationalProviderIdentifierNumber
        {
            NationalProviderIdentifierNumberId = Guid.CreateVersion7(),
            NationalProviderIdentifier = CalculateLuhnCheckDigit(GenerateMiddleDigits(GetFirstDigit(isOrganization))),
            CreatedAt = DateTime.UtcNow
        };
    
        return await _nationalProviderIdentifierRepository
            .AddAsync(npiNumber)
            .MapErrorAsync(NationalProviderIdentifierError (error) => error)
            .MapAsync(_ => new CreateNationalProviderIdentifierResponse
            {
                NationalProviderIdentifier = npiNumber.NationalProviderIdentifier
            });
    }

    public Result<ValidateNationalProviderIdentifierResponse, NationalProviderIdentifierError> ValidateNpi(string npi) =>
        ValidateNpiString(npi)
            .Ensure(
                n => n.Length == 10,
                new NationalProviderIdentifierValidationError("NPI must be exactly 10 digits"))
            .Ensure(
                n => long.TryParse(n, out _),
                new NationalProviderIdentifierValidationError("NPI must contain only numeric digits"))
            .Ensure(
                n => ValidateLuhnCheckDigit(n),
                new NationalProviderIdentifierValidationError("NPI failed Luhn check digit validation"))
            .Map(_ => new ValidateNationalProviderIdentifierResponse(true));

    private static StringBuilder GetFirstDigit(bool isOrganization)
        => new StringBuilder(10).Insert(0, isOrganization ? "2" : "1");

    private static StringBuilder GenerateMiddleDigits(StringBuilder input)
    {
        for (byte i = 0; i < 8; i++)
        {
            input.Append(Random.Shared.Next(0, 10));
        }
        
        return input;
    }

    private static string CalculateLuhnCheckDigit(StringBuilder input)
    {
        var luhnGeneratorString = new StringBuilder(14)
            .Append(LuhnCheckConstant)
            .Append(input);
        var sum = 0;
        var alternate = true;
        
        for (var i = luhnGeneratorString.Length - 1; i >= 0; i--) {
            var digit = int.Parse(luhnGeneratorString[i].ToString());
            if (alternate) {
                digit *= 2;
                if (digit > 9) {
                    digit = digit - 9;
                }
            }
            sum += digit;
            alternate = !alternate;
        }
        
        return input.Append((10 - (sum % 10)) % 10).ToString();
    }
    
    private static int CalculateLuhnCheckDigit(string input)
    {
        var luhnGeneratorString = new StringBuilder(14)
            .Append(LuhnCheckConstant)
            .Append(input);
        var sum = 0;
        var alternate = true;
        
        for (var i = luhnGeneratorString.Length - 1; i >= 0; i--) {
            var digit = int.Parse(luhnGeneratorString[i].ToString());
            if (alternate) {
                digit *= 2;
                if (digit > 9) {
                    digit = digit - 9;
                }
            }
            sum += digit;
            alternate = !alternate;
        }
        
        return (10 - (sum % 10)) % 10;
    }
    
    private static Result<string, NationalProviderIdentifierError> ValidateNpiString(string npi)
    {
        return string.IsNullOrEmpty(npi)
            ? Result<string, NationalProviderIdentifierError>.Failure(
                new NationalProviderIdentifierValidationError("NPI cannot be null or empty"))
            : Result<string, NationalProviderIdentifierError>.Success(npi);
    }
    
    private static bool ValidateLuhnCheckDigit(string npi)
    {
        var npiWithoutCheck = npi.Substring(0, 9);
        var providedCheckDigit = int.Parse(npi[9].ToString());
        var calculatedCheckDigit = CalculateLuhnCheckDigit(npiWithoutCheck);
        return providedCheckDigit == calculatedCheckDigit;
    }
}