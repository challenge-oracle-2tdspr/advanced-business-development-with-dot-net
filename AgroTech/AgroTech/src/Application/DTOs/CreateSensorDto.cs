namespace AgroTech.Application.DTOs
{
    public class CreateSensorDTO
    {
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tipo do sensor em formato numérico textual.
        /// Exemplos: "11", "12", "13", "14".
        /// </summary>
        public string Type { get; set; } = string.Empty;

        public double Value { get; set; }

        public DateTime Timestamp { get; set; }
    }
}