using BindSharp;
using MedicalUsersHelper.Logs;
using Photino.NET;

namespace MedicalUsersHelper.MessageHandlers;

/// <summary>
/// Routes incoming messages from JavaScript to the appropriate handler
/// </summary>
public sealed class MessageRouter
{
    private readonly IEnumerable<IMessageHandler> _handlers;
    private readonly IAppLogger _logger;
    private readonly Dictionary<string, IMessageHandler> _handlerMap;

    public MessageRouter(IEnumerable<IMessageHandler> handlers, IAppLogger logger)
    {
        _handlers = handlers;
        _logger = logger;
        
        _handlerMap = _handlers.ToDictionary(h => h.Command, h => h);
        
        _logger.LogInformation("Registered {0} message handlers: {1}", 
            _handlerMap.Count, 
            string.Join(", ", _handlerMap.Keys));
    }

    public void RouteMessage(PhotinoWindow window, string message)
    {
        _logger.LogDebug("Received message: {0}", message);

       ValidateMessage(message)
           .Tap(msg => _logger.LogDebug("Message validated: {0}", msg))
           .Bind(ParseMessage)
           .Tap(parsed => _logger.LogDebug("Parsed command: {0}", parsed.Command))
           .TapError(error => _logger.LogWarning("Invalid format: {0}", error))
           .Bind(parsed => GetHandler(parsed.Command)
               .Map(handler => (handler, parsed.Payload)))
           .Tap(tuple => _logger.LogDebug("Routing to {0} with payload: {1}", 
                tuple.handler.GetType().Name, tuple.Payload))
           .TapError(error => _logger.LogWarning("Error handling message: {0}", error))
           .Match(
               result =>
               {
                   ResultExtensions.Try(
                        () => {
                            result.handler.Handle(window, result.Payload);
                            return Unit.Value;
                        }
                    )
                    .TapError(ex => _logger.LogError(ex, "Error handling message: {0}", message))
                    .MapError(ex => $"Handler error - {ex.Message}")
                    .Match(
                        _ => Unit.Value,
                        error => {
                            window.SendWebMessage($"error:{error}");
                            return Unit.Value;
                        }
                    );
                    
                    return Unit.Value;
                },
                error =>
                {
                    _logger.LogWarning("Message routing failed: {0}", error);
                    window.SendWebMessage($"error:{error}");
                    return Unit.Value;
                }
            );
    }

    private Result<string, string> ValidateMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
            return Result<string, string>.Success(message);
        
        _logger.LogWarning("Received empty message");
        return Result<string, string>.Failure("Empty message received");
    }

    private Result<(string Command, string Payload), string> ParseMessage(string message)
    {
        int separatorIndex = message.IndexOf(':');
        
        if (separatorIndex == -1)
        {
            return $"Invalid message format. Expected 'command:payload'";
        }

        string command = message[..separatorIndex];
        string payload = message[(separatorIndex + 1)..];
        
        return (command, payload);
    }

    private Result<IMessageHandler, string> GetHandler(string command)
    {
        if (_handlerMap.TryGetValue(command, out var handler))
            return Result<IMessageHandler, string>.Success(handler);
        
        _logger.LogWarning("No handler found for command: {0}", command);
        return $"Unknown command '{command}'";
    }
}