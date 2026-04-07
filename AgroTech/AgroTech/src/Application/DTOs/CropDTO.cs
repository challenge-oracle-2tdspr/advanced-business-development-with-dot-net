using System;
using System.ComponentModel.DataAnnotations;

namespace AgroTech.Application.DTOs
{
    public class CropDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O nome da safra é obrigatório")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data de plantio é obrigatória")]
        public DateTime PlantingDate { get; set; }

        public DateTime? HarvestDate { get; set; }

        [Required(ErrorMessage = "A fazenda associada é obrigatória")]
        public Guid FarmId { get; set; }
    }
}