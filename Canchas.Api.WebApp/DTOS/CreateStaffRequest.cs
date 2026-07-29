using Canchas.Api.WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.WebApp.DTOS
{
    public class CreateStaffRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
        [Required]
        public string Password { get; set; } = "";
        [Required]
        public RolUsuario Rol { get; set; }
        public int? ClubId { get; set; } // Opcional para SuperAdmin, automático para ClubAdmin
    }
}
