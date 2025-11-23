using System.Text.Json.Serialization;

namespace Application.Core.DTOs.License.UI;

public sealed record LicenseRequest
{
    [JsonPropertyName("stateCode")]
    public required string StateCode { get; init; }
    [JsonPropertyName("lastName")]
    public required string LastName { get; init; }
    [JsonPropertyName("licenseType")]
    public required string LicenseType { get; init; }
    [JsonPropertyName("requestId")]
    public int RequestId { get; init; }
}