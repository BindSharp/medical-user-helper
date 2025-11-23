using System.Text.Json;
using Application.Core.DTOs.NationalProviderIdentifier.UI;
using Application.Core.Interfaces.NationalProviderIdentifier;
using MedicalUsersHelper.PhotinoHelpers;
using Photino.NET;

namespace MedicalUsersHelper.MessageHandlers.Handlers;

public sealed class NpiHandler : BaseMessageHandler
{
    private readonly INationalProviderIdentifier _npiService;
    
    public override string Command => "npi";

    public NpiHandler(INationalProviderIdentifier npiService)
    {
        _npiService = npiService;
    }

    public override void Handle(PhotinoWindow window, string payload)
    {
        try
        {
            // Extract JSON from "request:id:json" format
            var jsonPayload = ExtractJsonFromPayload(payload);
            
            // First try to determine if this is a generate or validate request
            var jsonDoc = JsonDocument.Parse(jsonPayload);
            var root = jsonDoc.RootElement;
            
            if (root.TryGetProperty("action", out var actionProp))
            {
                var action = actionProp.GetString();
                
                if (action == "validate")
                {
                    HandleValidateRequest(window, jsonPayload);
                }
                else if (action == "generate")
                {
                    HandleGenerateRequest(window, jsonPayload);
                }
                else
                {
                    window.SendError("npi:response:0", "Unknown action");
                }
            }
            else
            {
                window.SendError("npi:response:0", "Missing action property");
            }
        }
        catch (Exception ex)
        {
            window.SendError("npi:response:0", $"Error processing request: {ex.Message}");
        }
    }

    private async void HandleGenerateRequest(PhotinoWindow window, string payload)
    {
        try
        {
            var data = JsonSerializer.Deserialize<NpiGenerateRequest>(payload);
            
            if (data is null)
            {
                window.SendError("npi:response:" + data?.RequestId, "Invalid request data");
                return;
            }

            var result = await _npiService.CreateNationalProviderIdentifier(data.IsOrganization);

            if (result.IsSuccess)
            {
                window.SendJsonMessage($"npi:response:{data.RequestId}", new
                {
                    success = true,
                    npi = result.Value.NationalProviderIdentifier
                });
            }
            else
            {
                window.SendJsonMessage($"npi:response:{data.RequestId}", new
                {
                    success = false,
                    error = result.Error.Message
                });
            }
        }
        catch (Exception ex)
        {
            window.SendError("npi:response:0", $"Error generating NPI: {ex.Message}");
        }
    }

    private void HandleValidateRequest(PhotinoWindow window, string payload)
    {
        try
        {
            var data = JsonSerializer.Deserialize<NpiValidateRequest>(payload);
            
            if (data is null)
            {
                window.SendError("npi:response:" + data?.RequestId, "Invalid request data");
                return;
            }

            var result = _npiService.ValidateNpi(data.Npi);

            if (result.IsSuccess)
            {
                window.SendJsonMessage($"npi:response:{data.RequestId}", new
                {
                    success = true,
                    isValid = result.Value.isValid
                });
            }
            else
            {
                window.SendJsonMessage($"npi:response:{data.RequestId}", new
                {
                    success = false,
                    error = result.Error.Message
                });
            }
        }
        catch (Exception ex)
        {
            window.SendError("npi:response:0", $"Error validating NPI: {ex.Message}");
        }
    }
}