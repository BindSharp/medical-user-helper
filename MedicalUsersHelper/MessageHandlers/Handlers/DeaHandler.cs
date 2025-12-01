using Application.Core.DTOs.DrugEnforcementAdministration.UI;
using Application.Core.Interfaces.DrugEnforcementAdministration;
using BindSharp;
using Photino.NET;

namespace MedicalUsersHelper.MessageHandlers.Handlers;

public sealed class DeaHandler : BaseMessageHandler
{
    private readonly IDrugEnforcementAdministration _deaService;
    
    public override string Command => "dea";

    public DeaHandler(IDrugEnforcementAdministration deaService)
    {
        _deaService = deaService;
    }

    public override void Handle(PhotinoWindow window, string payload)
    {
        ExtractJsonFromPayload(payload)
            .Match(
                jsonPayload =>
                {
                    HandleRequest<DeaRequest>(window, jsonPayload, ProcessDeaRequest);
                    return Unit.Value;
                },
                error =>
                {
                    SendErrorResponse(window, 0, $"Error processing request: {error.Message}");
                    return Unit.Value;
                });
    }

    private async void ProcessDeaRequest(PhotinoWindow window, DeaRequest data)
    {
        if (data.IsNarcotic)
        {
            await HandleNdeaRequestAsync(window, data);
        }
        else
        {
            await HandleDeaRequestAsync(window, data);
        }
    }

    private async Task HandleDeaRequestAsync(PhotinoWindow window, DeaRequest data)
    {
        await _deaService.CreateDrugEnforcementAdministrationNumber(data.LastName)
            .MatchAsync(
                response => {
                    SendSuccessResponse(window, data.RequestId, "deaNumber", 
                        response.DrugEnforcementAdministrationNumber);
                    return Unit.Value;
                },
                error => {
                    SendErrorResponse(window, data.RequestId, error.Message);
                    return Unit.Value;
                }
            );
    }

    private async Task HandleNdeaRequestAsync(PhotinoWindow window, DeaRequest data)
    {
        await _deaService.CreateNarcoticDrugEnforcementAddictionNumber(data.LastName)
            .MatchAsync(
                response => {
                    SendSuccessResponse(window, data.RequestId, "deaNumber",
                        response.NarcoticDrugEnforcementAddictionNumber);
                    return Unit.Value;
                },
                error => {
                    SendErrorResponse(window, data.RequestId, error.Message);
                    return Unit.Value;
                }
            );
    }
}