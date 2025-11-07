using Photino.NET;

namespace MedicalUsersHelper.MessageHandlers;

/// <summary>
/// Interface for handling messages from JavaScript
/// </summary>
public interface IMessageHandler
{
    /// <summary>
    /// The command prefix this handler responds to (e.g., "user", "calculator")
    /// </summary>
    string Command { get; }
    
    /// <summary>
    /// Handle the message and optionally send a response
    /// </summary>
    /// <param name="window">The Photino window to send responses to</param>
    /// <param name="payload">The message payload (everything after "command:")</param>
    void Handle(PhotinoWindow window, string payload);
}