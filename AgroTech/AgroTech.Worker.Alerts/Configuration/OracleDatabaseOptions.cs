namespace AgroTech.Worker.Alerts.Configuration
{
    public class OracleDatabaseOptions
    {
        public const string SectionName = "OracleDatabase";

        public string ConnectionString { get; set; } = string.Empty;
    }
}