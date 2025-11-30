using Microsoft.Data.Sqlite;

namespace MedicalUsersHelper.DatabaseHelpers;

public sealed class DatabaseInitializer
{
    private readonly string _connectionString;
    private readonly string _databasePath;

    public DatabaseInitializer(string databasePath)
    {
        _databasePath = databasePath;
        _connectionString = $"Data Source={databasePath}";
    }

    /// <summary>
    /// Initialize the database - creates it if it doesn't exist and sets up the schema
    /// </summary>
    public void Initialize()
    {
        // Ensure directory exists
        string? directory = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Create database file if it doesn't exist
        if (!File.Exists(_databasePath))
        {
            Console.WriteLine($"[Database] Creating new database at: {_databasePath}");
            CreateDatabase();
        }
        else
        {
            Console.WriteLine($"[Database] Using existing database at: {_databasePath}");
            // Ensure all tables exist (in case of partial database)
            EnsureTablesExist();
        }
    }

    /// <summary>
    /// Create the database and all tables
    /// </summary>
    private void CreateDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var createTablesCommand = connection.CreateCommand();
        createTablesCommand.CommandText = GetCreateTablesScript();
        createTablesCommand.ExecuteNonQuery();

        Console.WriteLine("[Database] Database created successfully");
    }

    /// <summary>
    /// Ensure all tables exist (for existing databases)
    /// </summary>
    private void EnsureTablesExist()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var createTablesCommand = connection.CreateCommand();
        createTablesCommand.CommandText = GetCreateTablesScript();
        createTablesCommand.ExecuteNonQuery();
    }

    /// <summary>
    /// Get the SQL script to create all tables
    /// </summary>
    private static string GetCreateTablesScript()
    {
        return """
            -- DEA Registration Number Table
            CREATE TABLE IF NOT EXISTS "DeaRegistrationNumber" (
                "DeaRegistrationNumberId" TEXT PRIMARY KEY NOT NULL,
                "DeaRegistrationNumberValue" TEXT NOT NULL,
                "CreatedAt" TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS "idx_dea_value" ON "DeaRegistrationNumber"("DeaRegistrationNumberValue");
            CREATE INDEX IF NOT EXISTS "idx_dea_created" ON "DeaRegistrationNumber"("CreatedAt");

            -- NDEA Registration Number Table
            CREATE TABLE IF NOT EXISTS "NdeaRegistrationNumber" (
                "NdeaRegistrationNumberId" TEXT PRIMARY KEY NOT NULL,
                "NdeaRegistrationNumberValue" TEXT NOT NULL,
                "CreatedAt" TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS "idx_ndea_value" ON "NdeaRegistrationNumber"("NdeaRegistrationNumberValue");
            CREATE INDEX IF NOT EXISTS "idx_ndea_created" ON "NdeaRegistrationNumber"("CreatedAt");

            -- License Number Table
            CREATE TABLE IF NOT EXISTS "LicenseNumber" (
                "LicenseNumberId" TEXT PRIMARY KEY NOT NULL,
                "LicenseNumberValue" TEXT NOT NULL,
                "CreatedAt" TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS "idx_license_value" ON "LicenseNumber"("LicenseNumberValue");
            CREATE INDEX IF NOT EXISTS "idx_license_created" ON "LicenseNumber"("CreatedAt");

            -- National Provider Identifier Number Table
            CREATE TABLE IF NOT EXISTS "NationalProviderIdentifierNumber" (
                "NationalProviderIdentifierNumberId" TEXT PRIMARY KEY NOT NULL,
                "NationalProviderIdentifier" TEXT NOT NULL,
                "CreatedAt" TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS "idx_npi_value" ON "NationalProviderIdentifierNumber"("NationalProviderIdentifier");
            CREATE INDEX IF NOT EXISTS "idx_npi_created" ON "NationalProviderIdentifierNumber"("CreatedAt");
            """;
    }

    /// <summary>
    /// Test the database connection
    /// </summary>
    public bool TestConnection()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }
}