using BindSharp;
using Infrastructure.Core.DTOs.NationalProviderIdentifier;
using Infrastructure.Core.Models.NationalProviderIdentifier;

namespace Infrastructure.Core.Interfaces.NationalProviderIdentifier;

public interface INationalProviderIdentifierRepository
{
    Task<Result<Unit, NationalProviderIdentifierError>> AddAsync(NationalProviderIdentifierNumber npiNumber);
}