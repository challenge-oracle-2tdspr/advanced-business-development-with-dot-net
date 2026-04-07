using AgroTech.Domain.Common;
using System.Collections.Generic;

namespace AgroTech.Domain.Entities
{
    public class Farm : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        public ICollection<Crop> Crops { get; set; } = new List<Crop>();
        public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
    }
}