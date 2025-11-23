using System.Text.Json;
using Application.Core.DTOs.DrugEnforcementAdministration.UI;
using Application.Core.Interfaces.DrugEnforcementAdministration;
using MedicalUsersHelper.PhotinoHelpers;
using Photino.NET;

namespace MedicalUsersHelper.MessageHandlers.Handlers;

public sealed class DeaHandler: BaseMessageHandler
{
    private readonly IDrugEnforcementAdministration _deaService;
    
    public override string Command => "dea";

    public DeaHandler(IDrugEnforcementAdministration deaService)
    {
        _deaService = deaService;
    }

    public override void Handle(PhotinoWindow window, string payload)
    {
        try
        {
            // Extract JSON from "request:id:json" format
            var jsonPayload = ExtractJsonFromPayload(payload);
            
            var data = JsonSerializer.Deserialize<DeaRequest>(jsonPayload);
            
            if (data is null)
            {
                window.SendError("dea:response:" + data?.RequestId, "Invalid request data");
                return;
            }

            // Handle both regular DEA and NDEA (Narcotic)
            if (data.IsNarcotic)
            {
                HandleNdeaRequest(window, data);
            }
            else
            {
                HandleDeaRequest(window, data);
            }
        }
        catch (Exception ex)
        {
            window.SendError("dea:response:0", $"Error processing request: {ex.Message}");
        }
    }

    private async void HandleDeaRequest(PhotinoWindow window, DeaRequest data)
    {
        var result = await _deaService.CreateDrugEnforcementAdministrationNumber(data.LastName);

        if (result.IsSuccess)
        {
            window.SendJsonMessage($"dea:response:{data.RequestId}", new
            {
                success = true,
                deaNumber = result.Value.DrugEnforcementAdministrationNumber
            });
        }
        else
        {
            window.SendJsonMessage($"dea:response:{data.RequestId}", new
            {
                success = false,
                error = result.Error.Message
            });
        }
    }

    private async void HandleNdeaRequest(PhotinoWindow window, DeaRequest data)
    {
        var result = await _deaService.CreateNarcoticDrugEnforcementAddictionNumber(data.LastName);

        if (result.IsSuccess)
        {
            window.SendJsonMessage($"dea:response:{data.RequestId}", new
            {
                success = true,
                deaNumber = result.Value.NarcoticDrugEnforcementAddictionNumber
            });
        }
        else
        {
            window.SendJsonMessage($"dea:response:{data.RequestId}", new
            {
                success = false,
                error = result.Error.Message
            });
        }
    }
}