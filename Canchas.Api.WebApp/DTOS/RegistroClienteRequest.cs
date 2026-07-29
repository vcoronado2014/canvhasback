using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.WebApp.DTOS
{
    public class RegistroClienteRequest
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = "";

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = "";

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = "";

        [MaxLength(20)]
        public string? Telefono { get; set; }
    }
}
