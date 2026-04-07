using System;
using System.ComponentModel.DataAnnotations;

namespace AgroTech.Application.DTOs
{
    public class UserDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O nome do usuário é obrigatório")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido")]
        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = "Producer";
    }
}