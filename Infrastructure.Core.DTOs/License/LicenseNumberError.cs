namespace Infrastructure.Core.DTOs.License;

public abstract record LicenseNumberError(string Message, string? Details = null, Exception? Exception = null);

public sealed record LicenseNumberInsertError(string? Details = null, Exception? Exception = null)
    : LicenseNumberError("The license number was not saved in the database.", Details, Exception);
    
public sealed record LicenseNumberNoStateCodeError()
    : LicenseNumberError("No state code was provided for the license number creation.");
    
public sealed record LicenseNumberNoLastNameError()
    : LicenseNumberError("No last name was provided for the license number creation.");