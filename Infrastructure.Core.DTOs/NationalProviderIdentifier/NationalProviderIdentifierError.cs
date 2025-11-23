namespace Infrastructure.Core.DTOs.NationalProviderIdentifier;

public abstract record NationalProviderIdentifierError(string Message, string? Details = null, Exception? Exception = null);

public sealed record NationalProviderIdentifierInsertError(string? Details = null, Exception? Exception = null)
    : NationalProviderIdentifierError("The NPI number was not saved in the database.", Details, Exception);

public sealed record NationalProviderIdentifierValidationError(string? Details = null)
    : NationalProviderIdentifierError("The NPI validation failed.", Details);

public sealed record NationalProviderIdentifierBulkInsertError(string? Details = null, Exception? Exception = null)
    : NationalProviderIdentifierError("Non critical error: The NPI numbers were not saved in the database.", Details, Exception);
    
public sealed record GetAllNationalProviderIdentifiersError(string? Details = null, Exception? Exception = null)
    : NationalProviderIdentifierError("The NPIs could not be retrieved from the database.", Details, Exception);
    
public sealed record GetAllPrescribersNationalProviderIdentifiersError(string? Details = null, Exception? Exception = null)
    : NationalProviderIdentifierError("The prescribers NPIs could not be retrieved from the database.", Details, Exception);
    
public sealed record GetAllOrganizationsNationalProviderIdentifiersError(string? Details = null, Exception? Exception = null)
    : NationalProviderIdentifierError("The organizations NPIs could not be retrieved from the database.", Details, Exception);
    
public sealed record GetNationalProviderIdentifierError(string? Details = null, Exception? Exception = null)
    : NationalProviderIdentifierError("The NPI could not be retrieved from the database.", Details, Exception);
    
public sealed record NationalProviderIdentifierNotFound()
    : NationalProviderIdentifierError("No NPI could be found in the database.");