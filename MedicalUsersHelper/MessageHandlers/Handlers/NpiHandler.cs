using System.Text.Json;
using Application.Core.DTOs.NationalProviderIdentifier.UI;
using Application.Core.Interfaces.NationalProviderIdentifier;
using BindSharp;
using BindSharp.Extensions;
using MedicalUsersHelper.Logs;
using Photino.NET;

namespace MedicalUsersHelper.MessageHandlers.Handlers;

public sealed class NpiHandler : BaseMessageHandler
{
    private readonly IAppLogger _logger;
    
    private readonly INationalProviderIdentifier _npiService;
    
    public override string Command => "npi";

    public NpiHandler(INationalProviderIdentifier npiService, IAppLogger logger) : base(logger)
    {
        _logger = logger;
        _npiService = npiService;
    }

    public override void Handle(PhotinoWindow window, string payload)
    {
        var jsonPayload = ExtractJsonFromPayload(payload);
    
        var handlers = new Dictionary<string, Action>
        {
            ["validate"] = () => HandleRequest<NpiValidateRequest>(window, jsonPayload, ProcessValidateRequest),
            ["generate"] = () => HandleRequest<NpiGenerateRequest>(window, jsonPayload, ProcessGenerateRequest)
        };
    
        jsonPayload
            .MapError(err => err.Message)
            .Bind(json => Result.Try(
                () => JsonDocument.Parse(json),
                ex => $"JSON parsing failed: {ex.Message}"
            ))
            .Using(jsonDoc => 
            {
                JsonElement root = jsonDoc.RootElement;
            
                if (!root.TryGetProperty("action", out var actionProp))
                    return Result<string, string>.Failure("Missing action property");
            
                string? action = actionProp.GetString();
                return !string.IsNullOrEmpty(action)
                    ? Result<string, string>.Success(action)
                    : Result<string, string>.Failure("Action property is null");
            })
            .Bind(action => handlers.ContainsKey(action)
                ? Result<string, string>.Success(action)
                : Result<string, string>.Failure("Unknown action")
            )
            .Do(
                action => handlers[action](),
                error => SendErrorResponse(window, 0, error)
            );
    }

    private async void ProcessGenerateRequest(PhotinoWindow window, NpiGenerateRequest data) =>
        await _npiService.CreateNationalProviderIdentifier(data.IsOrganization)
            .DoAsync(
                response => SendSuccessResponse(window, data.RequestId, "npi", response.NationalProviderIdentifier),
                error => SendErrorResponse(window, data.RequestId, error.Message)
            );

    private void ProcessValidateRequest(PhotinoWindow window, NpiValidateRequest data) =>
        _npiService.ValidateNpi(data.Npi)
            .Do(
                response => SendSuccessResponse(window, data.RequestId, "isValid", response.isValid),
                error => SendErrorResponse(window, data.RequestId, error.Message)
            );
}