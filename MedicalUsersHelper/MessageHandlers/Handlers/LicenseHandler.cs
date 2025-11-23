using System.Text.Json;
using Application.Core.DTOs.License.UI;
using Application.Core.Interfaces.License;
using Infrastructure.Core.DTOs.License;
using MedicalUsersHelper.PhotinoHelpers;
using Photino.NET;

namespace MedicalUsersHelper.MessageHandlers.Handlers;

public sealed class LicenseHandler: BaseMessageHandler
{
    private readonly ILicenseNumber _licenseService;
    
    public override string Command => "license";

    public LicenseHandler(ILicenseNumber licenseService)
    {
        _licenseService = licenseService;
    }

    public override void Handle(PhotinoWindow window, string payload)
    {
        try
        {
            // Extract JSON from "request:id:json" format
            var jsonPayload = ExtractJsonFromPayload(payload);
            
            var data = JsonSerializer.Deserialize<LicenseRequest>(jsonPayload);
            
            if (data == null)
            {
                window.SendError("license:response:" + data?.RequestId, "Invalid request data");
                return;
            }

            HandleLicenseRequest(window, data);
        }
        catch (Exception ex)
        {
            window.SendError("license:response:0", $"Error processing request: {ex.Message}");
        }
    }

    private async void HandleLicenseRequest(PhotinoWindow window, LicenseRequest data)
    {
        var licenseType = data.LicenseType.ToLowerInvariant() == "pharmacy" 
            ? LicenseNumberType.Pharmacy 
            : LicenseNumberType.Medical;

        var result = await _licenseService.CreateLicenseNumber(
            data.StateCode, 
            data.LastName, 
            licenseType
        );

        if (result.IsSuccess)
        {
            window.SendJsonMessage($"license:response:{data.RequestId}", new
            {
                success = true,
                licenseNumber = result.Value.LicenseNumber
            });
        }
        else
        {
            window.SendJsonMessage($"license:response:{data.RequestId}", new
            {
                success = false,
                error = result.Error.Message
            });
        }
    }
}