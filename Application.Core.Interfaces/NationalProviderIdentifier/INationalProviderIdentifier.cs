using System.Collections.Immutable;
using Application.Core.DTOs.NationalProviderIdentifier;
using BindSharp;
using Infrastructure.Core.DTOs.NationalProviderIdentifier;
using Infrastructure.Core.Models.NationalProviderIdentifier;

namespace Application.Core.Interfaces.NationalProviderIdentifier;

public interface INationalProviderIdentifier
{
    Task<Result<CreateNationalProviderIdentifierResponse, NationalProviderIdentifierError>>
        CreateNationalProviderIdentifier(bool isOrganization);
    Result<ValidateNationalProviderIdentifierResponse, NationalProviderIdentifierError> ValidateNpi(string npi);
    Task<Result<ImmutableArray<NationalProviderIdentifierNumber>, NationalProviderIdentifierError>> GetAllAsync();
}