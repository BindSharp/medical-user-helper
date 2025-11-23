namespace Application.Core.DTOs.License;

public record CreateLicenseNumberResponse
{
    public required string LicenseNumber { get; init; }
}