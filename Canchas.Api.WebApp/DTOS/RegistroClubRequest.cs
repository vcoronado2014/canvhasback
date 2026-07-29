using Canchas.Api.WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.WebApp.DTOS
{
    public class RegistroClubRequest
    {

        //[Required(ErrorMessage = "El email del administrador es obligatorio")]
        //[EmailAddress(ErrorMessage = "El formato del email no es válido")]
        //[MaxLength(150)]
        //public string EmailAdmin { get; set; } = string.Empty;

        //[Required(ErrorMessage = "La contraseña es obligatoria")]
        //[MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        //[MaxLength(100)]
        //public string Password { get; set; } = string.Empty;


        // --- Datos Básicos del Club ---

        [Required(ErrorMessage = "El nombre del club es obligatorio")]
        [MaxLength(100)]
        public string NombreClub { get; set; } = string.Empty;

        [Required(ErrorMessage = "El subdominio es obligatorio para la URL personalizada")]
        [MaxLength(50)]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "El subdominio solo permite letras minúsculas, números y guiones")]
        public string Subdominio { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Direccion { get; set; } = string.Empty;

        public string? Telefono { get; set; }
        public string? Descripcion { get; set; }

        // Ubicación
        public string? RegionCodigo { get; set; }
        public string? RegionNombre { get; set; }
        public string? ComunaCodigo { get; set; }
        public string? ComunaNombre { get; set; }

        // Configuración adicional
        public List<string>? MetodosPagoHabilitados { get; set; } = new() { "Efectivo" };
        public EstadoSuscripcionClub EstadoSuscripcion { get; set; } = EstadoSuscripcionClub.PendientePago;
        public DateTime? FechaProxVencimiento { get; set; }
    }
}
