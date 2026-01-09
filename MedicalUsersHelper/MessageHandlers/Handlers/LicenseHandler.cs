using Application.Core.DTOs.License.UI;
using Application.Core.Interfaces.License;
using BindSharp;
using BindSharp.Extensions;
using Infrastructure.Core.DTOs.License;
using MedicalUsersHelper.Logs;
using Photino.NET;

namespace MedicalUsersHelper.MessageHandlers.Handlers;

public sealed class LicenseHandler : BaseMessageHandler
{
    private readonly IAppLogger _logger;
    
    private readonly ILicenseNumber _licenseService;
    
    public override string Command => "license";

    public LicenseHandler(ILicenseNumber licenseService, IAppLogger logger) : base(logger)
    {
        _logger = logger;
        _licenseService = licenseService;
    }

    public override void Handle(PhotinoWindow window, string payload) =>
        ExtractJsonFromPayload(payload)
            .Do(
                jsonPayload => HandleRequest<LicenseRequest>(window, jsonPayload, ProcessLicenseRequest),
                error => SendErrorResponse(window, 0, $"Error processing request: {error.Message}")
            );

    private async void ProcessLicenseRequest(PhotinoWindow window, LicenseRequest data) =>
        await _licenseService.CreateLicenseNumber(
                data.StateCode,
                data.LastName,
                GetLicenseNumberType(data.LicenseType)
            )
            .DoAsync(
                response => SendSuccessResponse(window, data.RequestId, "licenseNumber", response.LicenseNumber),
                error => SendErrorResponse(window, data.RequestId, error.Message)
            );

    private static LicenseNumberType GetLicenseNumberType(string licenseType) =>
        licenseType.ToLowerInvariant() switch
        {
            "pharmacy" => LicenseNumberType.Pharmacy,
            "medical" => LicenseNumberType.Medical,
            _ => LicenseNumberType.Medical
        };
}