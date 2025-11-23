using System.Text.Json.Serialization;

namespace Application.Core.DTOs.DrugEnforcementAdministration.UI;

public sealed record DeaRequest
{
    [JsonPropertyName("lastName")]
    public required string LastName { get; init; }
    [JsonPropertyName("isNarcotic")]
    public bool IsNarcotic { get; init; }
    [JsonPropertyName("requestId")]
    public int RequestId { get; init; }
}