using Canchas.Api.WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.WebApp.DTOS
{
    public class UserReadDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Telefono { get; set; }
        public RolUsuario Rol { get; set; }
        public int? ClubId { get; set; }
        public string? NombreClub { get; set; }
    }

    public class UserCreateDto
    {
        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = "";

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = "";

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; } = "";

        [Required]
        public RolUsuario Rol { get; set; }

        public int? ClubId { get; set; }
    }

    public class UserUpdateDto
    {
        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = "";

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = "";

        [MaxLength(20)]
        public string? Telefono { get; set; }

        // Opcional: solo si desea cambiar la clave
        [MinLength(6)]
        public string? Password { get; set; }

        [Required]
        public RolUsuario Rol { get; set; }

        public int? ClubId { get; set; }
    }
}
