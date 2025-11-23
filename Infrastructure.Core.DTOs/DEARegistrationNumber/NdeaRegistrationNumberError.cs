namespace Infrastructure.Core.DTOs.DEARegistrationNumber;

public abstract record NdeaRegistrationNumberError(string Message, string? Details = null, Exception? Exception = null);

public sealed record NdeaRegistrationNumberInsertError(string? Details = null, Exception? Exception = null)
    : NdeaRegistrationNumberError("The NDEA number was not saved in the database.", Details, Exception);
    
public sealed record NdeaRegistrationNumberNoLastNameError()
    : NdeaRegistrationNumberError("No last name was provided for the NDEA number creation.");