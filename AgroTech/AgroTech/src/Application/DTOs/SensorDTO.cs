using System;
using System.ComponentModel.DataAnnotations;

namespace AgroTech.Application.DTOs
{
    public class SensorDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O nome do sensor é obrigatório")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo do sensor é obrigatório")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "O valor do sensor é obrigatório")]
        public double Value { get; set; }

        [Required(ErrorMessage = "O timestamp é obrigatório")]
        public DateTime Timestamp { get; set; }
    }
}