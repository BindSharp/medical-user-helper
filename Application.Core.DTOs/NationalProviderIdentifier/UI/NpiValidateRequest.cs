using System.Text.Json.Serialization;

namespace Application.Core.DTOs.NationalProviderIdentifier.UI;

public sealed record NpiValidateRequest
{
    [JsonPropertyName("npi")]
    public required string Npi { get; init; }
    [JsonPropertyName("action")]
    public required string Action { get; init; }
    [JsonPropertyName("requestId")]
    public int RequestId { get; init; }
}