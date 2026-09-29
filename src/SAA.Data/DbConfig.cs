namespace SAA.Data;

/// <summary>
/// Central connection-string resolution. The default targets a local
/// SQL Server Express instance (as used in SSMS). Override with the
/// SAA_CONNECTION environment variable without rebuilding, which is how
/// the app is pointed at LocalDB or another instance.
/// </summary>
public static class DbConfig
{
    public const string DefaultConnectionString =
        @"Server=.\SQLEXPRESS;Database=SAA_FlightBooking;Trusted_Connection=True;TrustServerCertificate=True;";

    public static string Resolve()
    {
        var env = Environment.GetEnvironmentVariable("SAA_CONNECTION");
        return string.IsNullOrWhiteSpace(env) ? DefaultConnectionString : env;
    }
}
