using System.ComponentModel.DataAnnotations;

namespace AgroTech.Application.DTOs
{
    public class FarmDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O nome da fazenda é obrigatório")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "A localização é obrigatória")]
        public string Location { get; set; } = string.Empty;
    }
}