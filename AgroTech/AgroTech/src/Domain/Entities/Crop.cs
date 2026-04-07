using AgroTech.Domain.Common;
using System;

namespace AgroTech.Domain.Entities
{
    public class Crop : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public DateTime PlantingDate { get; set; }
        public DateTime? HarvestDate { get; set; }

        public Guid FarmId { get; set; }
        public Farm? Farm { get; set; }
    }
}