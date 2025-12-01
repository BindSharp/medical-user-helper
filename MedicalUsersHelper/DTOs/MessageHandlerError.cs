namespace MedicalUsersHelper.DTOs;

public abstract record MessageHandlerError(string Message, string? Details = null, Exception? Exception = null);

public sealed record PayloadParseError(string? Details = null, Exception? Exception = null)
    : MessageHandlerError("There was an internal problem retrieving the payload information. See the details for more information.", Details, Exception);
    
public sealed record EmptyPayloadError()
    : MessageHandlerError("The payload was empty. Thus, no information can be retrieved.");