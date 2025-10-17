using AgroTech.Domain.Common;
using AgroTech.Domain.Enums;

namespace AgroTech.Domain.Entities
{
    public class Sensor : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public SensorType Type { get; set; }
        public string Unit { get; set; } = string.Empty;
        public double Value { get; set; }
        public Guid FarmId { get; set; }
        public Farm? Farm { get; set; }

        public void UpdateValue(double newValue)
        {
            Value = newValue;
            UpdatedAt = DateTime.UtcNow;


        }
    }
}

