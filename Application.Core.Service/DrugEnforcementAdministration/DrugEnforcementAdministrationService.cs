using System.Text;
using Application.Core.DTOs.DrugEnforcementAdministration;
using Application.Core.Interfaces.DrugEnforcementAdministration;
using BindSharp;
using Infrastructure.Core.DTOs.DEARegistrationNumber;
using Infrastructure.Core.Interfaces.DEARegistrationNumber;
using Infrastructure.Core.Models.DEARegistrationNumber;

namespace Application.Core.Service.DrugEnforcementAdministration;

public sealed class DrugEnforcementAdministrationService : BaseService, IDrugEnforcementAdministration
{
    private readonly IDeaRegistrationNumberRepository _deaRegistrationNumberRepository;
    private readonly INdeaRegistrationNumberRepository _ndeaRegistrationNumberRepository;
    
    private static readonly char[] DeaRegistrantTypes = ['A', 'B', 'F'];
    private static readonly char[] NdearegistrantTypes = ['M', 'P'];
    
    public DrugEnforcementAdministrationService(IDeaRegistrationNumberRepository deaRegistrationNumberRepository,
        INdeaRegistrationNumberRepository ndeaRegistrationNumberRepository)
    {
        _deaRegistrationNumberRepository = deaRegistrationNumberRepository;
        _ndeaRegistrationNumberRepository = ndeaRegistrationNumberRepository;
    }

    public async Task<Result<CreateDrugEnforcementAdministrationNumberResponse, DeaRegistrationNumberError>>
        CreateDrugEnforcementAdministrationNumber(string lastName)
    {
        var deaNumber = new DeaRegistrationNumber
        {
            DeaRegistrationNumberId = Guid.CreateVersion7(),
            DeaRegistrationNumberValue = GenerateDeaNumber(lastName),
            CreatedAt = DateTime.UtcNow
        };
        
        return await IsStringEmpty<DeaRegistrationNumberNoLastNameError>(lastName)
            .MapError(DeaRegistrationNumberError (error) => error)
            .BindAsync(async _ => await _deaRegistrationNumberRepository.AddAsync(deaNumber))
            .MapAsync(_ => new CreateDrugEnforcementAdministrationNumberResponse
            {
                DrugEnforcementAdministrationNumber = deaNumber.DeaRegistrationNumberValue
            });
    }

    public async Task<Result<NarcoticDrugEnforcementAddictionNumberResponse, NdeaRegistrationNumberError>>
        CreateNarcoticDrugEnforcementAddictionNumber(string lastName)
    {
        var ndeaNumber = new NarcoticDrugEnforcementAddictionNumber
        {
            NarcoticDrugEnforcementAddictionNumberId =  Guid.CreateVersion7(),
            NarcoticDrugEnforcementAddictionNumberValue = GenerateNdeaNumber(lastName),
            CreatedAt = DateTime.UtcNow
        };

        return await IsStringEmpty<NdeaRegistrationNumberNoLastNameError>(lastName)
            .MapError(NdeaRegistrationNumberError (error) => error)
            .BindAsync(async _ => await _ndeaRegistrationNumberRepository.AddAsync(ndeaNumber))
            .MapAsync(_ => new NarcoticDrugEnforcementAddictionNumberResponse
            {
                NarcoticDrugEnforcementAddictionNumber = ndeaNumber.NarcoticDrugEnforcementAddictionNumberValue
            });
    }

    private static string GenerateDeaNumber(string lastName) {
        var registrantType = DeaRegistrantTypes[Random.Shared.Next(DeaRegistrantTypes.Length)];
        var lastNameInitial = char.ToUpper(lastName[0]);
        var sixDigits = Random.Shared.Next(100000, 999999).ToString();
        var checksum = CalculateDeaChecksum(sixDigits);
        
        return string.Create(9, (registrantType, lastNameInitial, sixDigits, checksum), 
            (span, state) =>
            {
                span[0] = state.registrantType;
                span[1] = state.lastNameInitial;
                state.sixDigits.AsSpan().CopyTo(span.Slice(2, 6));
                span[8] = (char)('0' + state.checksum);
            });
    }
    
    private static int CalculateDeaChecksum(string sixDigits)
    {
        // Add the 1st, 3rd, and 5th digits
        var sum1 = (sixDigits[0] - '0') + (sixDigits[2] - '0') + (sixDigits[4] - '0');
        // Add the 2nd, 4th, and 6th digits and multiply by 2
        var sum2 = ((sixDigits[1] - '0') + (sixDigits[3] - '0') + (sixDigits[5] - '0')) * 2;
        // The checksum is the last digit of the total return total % 10;
        return (sum1 + sum2) % 10;
    }
    
    private static string GenerateNdeaNumber(string lastName) {
        var registrantType = NdearegistrantTypes[Random.Shared.Next(NdearegistrantTypes.Length)];
        var lastNameInitial = char.ToUpper(lastName[0]);
        var sixDigits = Random.Shared.Next(100000, 999999).ToString();
        var checksum = CalculateDeaChecksum(sixDigits);
        
        return string.Create(9, (registrantType, lastNameInitial, sixDigits, checksum), 
            (span, state) =>
            {
                span[0] = state.registrantType;
                span[1] = state.lastNameInitial;
                state.sixDigits.AsSpan().CopyTo(span.Slice(2, 6));
                span[8] = (char)('0' + state.checksum);
            });
    }
}