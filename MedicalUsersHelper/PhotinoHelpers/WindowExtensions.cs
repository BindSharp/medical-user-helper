using System.Text.Json;
using Photino.NET;

namespace MedicalUsersHelper.PhotinoHelpers;

/// <summary>
/// Extension methods for PhotinoWindow to make message sending easier
/// </summary>
public static class WindowExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Send a JSON message to JavaScript
    /// Format: "command:jsonPayload"
    /// </summary>
    public static void SendJsonMessage<T>(this PhotinoWindow window, string command, T data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        var message = $"{command}:{json}";
        window.SendWebMessage(message);
    }

    /// <summary>
    /// Send a simple text message to JavaScript
    /// Format: "command:text"
    /// </summary>
    public static void SendTextMessage(this PhotinoWindow window, string command, string text)
    {
        window.SendWebMessage($"{command}:{text}");
    }

    /// <summary>
    /// Send a success response
    /// </summary>
    public static void SendSuccess(this PhotinoWindow window, string command, string message = "Success")
    {
        window.SendJsonMessage(command, new { success = true, message });
    }

    /// <summary>
    /// Send an error response
    /// </summary>
    public static void SendError(this PhotinoWindow window, string command, string error)
    {
        window.SendJsonMessage(command, new { success = false, error });
    }
}