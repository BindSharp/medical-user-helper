using System.Text.Json;
using BindSharp;
using MedicalUsersHelper.DTOs;
using MedicalUsersHelper.Logs;
using MedicalUsersHelper.PhotinoHelpers;
using Photino.NET;

namespace MedicalUsersHelper.MessageHandlers;

/// <summary>
/// Base class for message handlers with common functionality
/// </summary>
public abstract class BaseMessageHandler : IMessageHandler
{
    private readonly IAppLogger _logger;
    public abstract string Command { get; }
    public abstract void Handle(PhotinoWindow window, string payload);

    protected BaseMessageHandler(IAppLogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extract JSON from payload that may be in "request:id:json" format
    /// </summary>
    protected static Result<string, MessageHandlerError> ExtractJsonFromPayload(string payload) =>
        ValidateIfPayloadIsEmpty(payload)
            .Map(trimmedPayload => trimmedPayload.TrimStart())
            .BindIf(
                trimmed => !(trimmed.StartsWith('{') || trimmed.StartsWith('[')),
                trimmed => ExtractJsonAfterPrefix(trimmed)
            );

    /// <summary>
    /// Deserialize JSON payload and execute an action with error handling
    /// </summary>
    protected void HandleRequest<TRequest>(PhotinoWindow window, Result<string, MessageHandlerError> jsonPayload, Action<PhotinoWindow, TRequest> handler)
        where TRequest : class
    {
        jsonPayload
            .MapError(err => err.Message)
            .Bind(json => ResultExtensions.Try(
                () => JsonSerializer.Deserialize<TRequest>(json),
                ex => $"Deserialization failed: {ex.Message}"
            ))
            .EnsureNotNull("Invalid request data")
            .Tap(result => _logger.LogDebug($"Request successful: {Command}:response:0:{{0}}", result))
            .TapError(error => _logger.LogDebug($"Error handling request: {Command}:response:0:{{0}}", error))
            .Match(
                data => {
                    handler(window, data);
                    return Unit.Value;
                },
                error => {
                    window.SendError(_logger, $"{Command}:response:0", error);
                    return Unit.Value;
                }
            );
    }

    /// <summary>
    /// Send a success response with a result value
    /// </summary>
    protected void SendSuccessResponse<T>(PhotinoWindow window, int requestId, string propertyName, T value)
    {
        window.SendJsonMessage(_logger, $"{Command}:response:{requestId}", new Dictionary<string, object>
        {
            ["success"] = true,
            [propertyName] = value!
        });
    }

    /// <summary>
    /// Send an error response
    /// </summary>
    protected void SendErrorResponse(PhotinoWindow window, int requestId, string errorMessage)
    {
        window.SendJsonMessage(_logger, $"{Command}:response:{requestId}", new
        {
            success = false,
            error = errorMessage
        });
    }

    private static Result<string, MessageHandlerError> ValidateIfPayloadIsEmpty(string payload) =>
        string.IsNullOrEmpty(payload) || string.IsNullOrWhiteSpace(payload)
            ? new EmptyPayloadError()
            : payload;

    private static Result<string, MessageHandlerError> ExtractJsonAfterPrefix(string payload)
    {
        int firstColon = payload.IndexOf(':');
        if (firstColon == -1)
        {
            return payload;
        }

        int secondColon = payload.IndexOf(':', firstColon + 1);
        
        return secondColon == -1
            ? payload[(firstColon + 1)..]
            : payload[(secondColon + 1)..];
    }
}