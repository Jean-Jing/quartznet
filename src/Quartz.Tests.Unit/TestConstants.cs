namespace Quartz;

public static class TestConstants
{
    static TestConstants()
    {
        SqlServerUser = Environment.GetEnvironmentVariable("MSSQL_USER") ?? "sa";
        SqlServerPassword = Environment.GetEnvironmentVariable("MSSQL_PASSWORD") ?? "SqlServer@123";
        // we cannot use trusted connection as it's not available for Linux provider
        SqlServerConnectionString = $"Server=localhost;Database=quartznet;User Id={SqlServerUser};Password={SqlServerPassword};TrustServerCertificate=True";
        SqlServerConnectionStringMOT = $"Server=localhost,1444;Database=quartznet;User Id={SqlServerUser};Password={SqlServerPassword};TrustServerCertificate=True";

        PostgresUser = Environment.GetEnvironmentVariable("PG_USER") ?? "postgres";
        PostgresPassword = Environment.GetEnvironmentVariable("PG_PASSWORD") ?? "xiaomao01@123";
        PostgresConnectionString = $"Server=127.0.0.1;Port=5432;Userid={PostgresUser};Password={PostgresPassword};Pooling=true;MinPoolSize=1;MaxPoolSize=20;Timeout=15;SslMode=Disable;Database=quartz";
    }

    public static string SqlServerUser { get; }
    public static string SqlServerPassword { get; }

    public static string SqlServerConnectionString { get; }
    public static string SqlServerConnectionStringMOT { get; }

    public static string PostgresUser { get; }
    public static string PostgresPassword { get; }
    public static string PostgresConnectionString { get; }

    public const string DefaultSerializerType = "json";

    public const string DefaultSqlServerProvider = "SqlServer";

    public const string PostgresProvider = "Npgsql";
}