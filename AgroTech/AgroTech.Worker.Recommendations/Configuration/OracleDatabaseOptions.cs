namespace AgroTech.Worker.Recommendations.Configuration
{
    public class OracleDatabaseOptions
    {
        public const string SectionName = "OracleDatabase";

        public string ConnectionString { get; set; } = string.Empty;
    }
}