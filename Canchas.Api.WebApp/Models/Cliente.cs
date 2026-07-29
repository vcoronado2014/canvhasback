using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.WebApp.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = "";

        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(256)]
        public string? PasswordHash { get; set; }

        public List<Reserva> Reservas { get; set; } = [];
    }
}
