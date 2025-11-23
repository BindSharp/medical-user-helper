namespace Infrastructure.Core.Services.License;

public static class LicenseNumberSql
{
    public const string Insert = """
                                 INSERT INTO "LicenseNumber"
                                     ("LicenseNumberId", "LicenseNumberValue", "CreatedAt")
                                 VALUES
                                     (@LicenseNumberId, @LicenseNumberValue, @CreatedAt)
                                 """;
}