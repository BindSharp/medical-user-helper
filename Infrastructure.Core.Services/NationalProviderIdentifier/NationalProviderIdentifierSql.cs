namespace Infrastructure.Core.Services.NationalProviderIdentifier;

public static class NationalProviderIdentifierSql
{
    public const string Insert = """
                                 INSERT INTO "NationalProviderIdentifierNumber"
                                     ("NationalProviderIdentifierNumberId", "NationalProviderIdentifier", "CreatedAt")
                                 VALUES
                                     (@NationalProviderIdentifierNumberId, @NationalProviderIdentifier, @CreatedAt)
                                 """;
}