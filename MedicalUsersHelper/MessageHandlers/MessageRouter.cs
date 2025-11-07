using Photino.NET;

namespace MedicalUsersHelper.MessageHandlers;

/// <summary>
/// Routes incoming messages from JavaScript to the appropriate handler
/// </summary>
public class MessageRouter
{
    private readonly IEnumerable<IMessageHandler> _handlers;
    //private readonly ILogger<MessageRouter> _logger;
    private readonly Dictionary<string, IMessageHandler> _handlerMap;

    public MessageRouter(IEnumerable<IMessageHandler> handlers/*, ILogger<MessageRouter> logger*/)
    {
        _handlers = handlers;
        //_logger = logger;
        
        // Build a map of command -> handler for fast lookup
        _handlerMap = handlers.ToDictionary(h => h.Command, h => h);
        
        /*_logger.LogInformation("Registered {Count} message handlers: {Commands}", 
            _handlerMap.Count, 
            string.Join(", ", _handlerMap.Keys));*/
    }

    /// <summary>
    /// Routes a message to the appropriate handler
    /// Format: "command:payload"
    /// </summary>
    public void RouteMessage(PhotinoWindow window, string message)
    {
        //_logger.LogDebug("Received message: {Message}", message);

        if (string.IsNullOrWhiteSpace(message))
        {
            //_logger.LogWarning("Received empty message");
            return;
        }

        var separatorIndex = message.IndexOf(':');
        
        if (separatorIndex == -1)
        {
            //_logger.LogWarning("Invalid message format (missing ':'): {Message}", message);
            window.SendWebMessage($"error:Invalid message format. Expected 'command:payload'");
            return;
        }

        var command = message[..separatorIndex];
        var payload = message[(separatorIndex + 1)..];

        if (_handlerMap.TryGetValue(command, out var handler))
        {
            try
            {
                /*_logger.LogDebug("Routing to {Handler} with payload: {Payload}", 
                    handler.GetType().Name, payload);*/
                handler.Handle(window, payload);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error handling message: {Message}", message);
                window.SendWebMessage($"error:Handler error - {ex.Message}");
            }
        }
        else
        {
            //_logger.LogWarning("No handler found for command: {Command}", command);
            window.SendWebMessage($"error:Unknown command '{command}'");
        }
    }
}