using AgroTech.Domain.Common;
using System;

namespace AgroTech.Domain.Entities
{
    public class Sensor : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Type { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }
}