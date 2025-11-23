using System.Text.Json.Serialization;

namespace Application.Core.DTOs.NationalProviderIdentifier.UI;

public sealed record NpiGenerateRequest
{
    [JsonPropertyName("isOrganization")]
    public bool IsOrganization { get; init; }
    [JsonPropertyName("action")]
    public required string Action { get; init; }
    [JsonPropertyName("requestId")]
    public int RequestId { get; init; }
}