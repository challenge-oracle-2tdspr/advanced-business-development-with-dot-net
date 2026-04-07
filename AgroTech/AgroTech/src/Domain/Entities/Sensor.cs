using AgroTech.Domain.Common;
using System;

namespace AgroTech.Domain.Entities
{
    public class Sensor : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Type { get; set; }
        public string Unit { get; set; } = string.Empty;
        public double Value { get; set; }

        public Guid FarmId { get; set; }
        public Farm? Farm { get; set; }
    }
}