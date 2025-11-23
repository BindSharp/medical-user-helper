namespace Infrastructure.Core.DTOs.DEARegistrationNumber;

public abstract record DeaRegistrationNumberError(string Message, string? Details = null, Exception? Exception = null);

public sealed record DeaRegistrationNumberInsertError(string? Details = null, Exception? Exception = null)
    : DeaRegistrationNumberError("The DEA number was not saved in the database.", Details, Exception);
    
public sealed record DeaRegistrationNumberNoLastNameError()
    : DeaRegistrationNumberError("No last name was provided for the DEA number creation.");