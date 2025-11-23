namespace Infrastructure.Core.Services.DEARegistrationNumber;

public static class NdeaRegistrationNumberSql
{
    public const string Insert = """
                                 INSERT INTO "NdeaRegistrationNumber"
                                     ("NdeaRegistrationNumberId", "NdeaRegistrationNumberValue", "CreatedAt")
                                 VALUES
                                     (@NdeaRegistrationNumberId, @NdeaRegistrationNumberValue, @CreatedAt)
                                 """;
}