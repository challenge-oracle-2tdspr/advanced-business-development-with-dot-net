namespace AgroTech.Worker.Readings.Configuration
{
    public class OracleDatabaseOptions
    {
        public const string SectionName = "OracleDatabase";

        public string ConnectionString { get; set; } = string.Empty;
    }
}
