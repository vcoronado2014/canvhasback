using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.WebApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Email { get; set; } = "";

        [MaxLength(150)]
        public string Nombre { get; set; } = "";

        [MaxLength(20)]
        public string? Telefono { get; set; } = "";

        [MaxLength(256)]
        public string? PasswordHash { get; set; }

        public RolUsuario Rol { get; set; }

        public int? ClubId { get; set; }
        public Club? Club { get; set; }
    }
}
