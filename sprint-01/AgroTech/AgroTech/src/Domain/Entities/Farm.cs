using AgroTech.Domain.Common;

namespace AgroTech.Domain.Entities
{
    public class Farm : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public List<Sensor> Sensors { get; set; } = new();
        public List<Crop> Crops { get; set; } = new();
    }
}