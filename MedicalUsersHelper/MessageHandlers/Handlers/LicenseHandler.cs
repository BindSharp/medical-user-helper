using System.Text.Json;
using Application.Core.DTOs.License.UI;
using Application.Core.Interfaces.License;
using BindSharp;
using Infrastructure.Core.DTOs.License;
using Photino.NET;

namespace MedicalUsersHelper.MessageHandlers.Handlers;

public sealed class LicenseHandler : BaseMessageHandler
{
    private readonly ILicenseNumber _licenseService;
    
    public override string Command => "license";

    public LicenseHandler(ILicenseNumber licenseService)
    {
        _licenseService = licenseService;
    }

    public override void Handle(PhotinoWindow window, string payload)
    {
        ExtractJsonFromPayload(payload)
            .Match(
                jsonPayload =>
                {
                    HandleRequest<LicenseRequest>(window, jsonPayload, ProcessLicenseRequest);
                    return Unit.Value;
                },
                error =>
                {
                    SendErrorResponse(window, 0, $"Error processing request: {error.Message}");
                    return Unit.Value;
                }
            );
    }

    private async void ProcessLicenseRequest(PhotinoWindow window, LicenseRequest data)
    {
        await _licenseService.CreateLicenseNumber(
                data.StateCode,
                data.LastName,
                GetLicenseNumberType(data.LicenseType)
            )
            .MatchAsync(
                response =>
                {
                    SendSuccessResponse(window, data.RequestId, "licenseNumber",
                        response.LicenseNumber);
                    return Unit.Value;
                },
                error =>
                {
                    SendErrorResponse(window, data.RequestId, error.Message);
                    return Unit.Value;
                }
            );
    }

    private static LicenseNumberType GetLicenseNumberType(string licenseType) =>
        licenseType.ToLowerInvariant() switch
        {
            "pharmacy" => LicenseNumberType.Pharmacy,
            "medical" => LicenseNumberType.Medical,
            _ => LicenseNumberType.Medical
        };
}