using Application.Core.DTOs.DrugEnforcementAdministration.UI;
using Application.Core.Interfaces.DrugEnforcementAdministration;
using BindSharp;
using BindSharp.Extensions;
using MedicalUsersHelper.Logs;
using Photino.NET;

namespace MedicalUsersHelper.MessageHandlers.Handlers;

public sealed class DeaHandler : BaseMessageHandler
{
    private readonly IAppLogger _logger;
    
    private readonly IDrugEnforcementAdministration _deaService;
    
    public override string Command => "dea";

    public DeaHandler(IDrugEnforcementAdministration deaService, IAppLogger logger) : base(logger)
    {
        _logger = logger;
        _deaService = deaService;
    }

    public override void Handle(PhotinoWindow window, string payload) =>
        ExtractJsonFromPayload(payload)
            .Do(
                jsonPayload => HandleRequest<DeaRequest>(window, jsonPayload, ProcessDeaRequest),
                error => SendErrorResponse(window, 0, $"Error processing request: {error.Message}")
            );

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

    private async Task HandleDeaRequestAsync(PhotinoWindow window, DeaRequest data) =>
        await _deaService.CreateDrugEnforcementAdministrationNumber(data.LastName)
            .DoAsync(
                response => SendSuccessResponse(window, data.RequestId, "deaNumber", response.DrugEnforcementAdministrationNumber),
                error => SendErrorResponse(window, data.RequestId, error.Message)
            );

    private async Task HandleNdeaRequestAsync(PhotinoWindow window, DeaRequest data) =>
        await _deaService.CreateNarcoticDrugEnforcementAddictionNumber(data.LastName)
            .DoAsync(
                response => SendSuccessResponse(window, data.RequestId, "deaNumber", response.NarcoticDrugEnforcementAddictionNumber),
                error => SendErrorResponse(window, data.RequestId, error.Message)
            );
}