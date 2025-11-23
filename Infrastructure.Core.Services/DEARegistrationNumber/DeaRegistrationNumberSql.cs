namespace Infrastructure.Core.Services.DEARegistrationNumber;

public static class DeaRegistrationNumberSql
{
    public const string Insert = """
                                 INSERT INTO "DeaRegistrationNumber"
                                     ("DeaRegistrationNumberId", "DeaRegistrationNumberValue", "CreatedAt")
                                 VALUES
                                     (@DeaRegistrationNumberId, @DeaRegistrationNumberValue, @CreatedAt)
                                 """;
}