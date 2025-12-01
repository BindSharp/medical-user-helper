using System.Text.Json;
using Application.Core.DTOs.NationalProviderIdentifier.UI;
using Application.Core.Interfaces.NationalProviderIdentifier;
using BindSharp;
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
            .Bind(json => ResultExtensions.Try(
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
            .Match(
                action =>
                {
                    handlers[action]();
                    return Unit.Value;
                },
                error =>
                {
                    SendErrorResponse(window, 0, error);
                    return Unit.Value;
                }
            );
    }

    private async void ProcessGenerateRequest(PhotinoWindow window, NpiGenerateRequest data)
    {
        await _npiService.CreateNationalProviderIdentifier(data.IsOrganization)
            .MatchAsync(
                response =>
                {
                    SendSuccessResponse(window, data.RequestId, "npi",
                        response.NationalProviderIdentifier);
                    return Unit.Value;
                },
                error =>
                {
                    SendErrorResponse(window, data.RequestId, error.Message);
                    return Unit.Value;
                }
            );
    }

    private void ProcessValidateRequest(PhotinoWindow window, NpiValidateRequest data)
    {
        _npiService.ValidateNpi(data.Npi)
            .Match(
                response =>
                {
                    SendSuccessResponse(window, data.RequestId, "isValid", response.isValid);
                    return Unit.Value;
                },
                    error =>
                {
                    SendErrorResponse(window, data.RequestId, error.Message);
                    return Unit.Value;
                }
            );
    }
}