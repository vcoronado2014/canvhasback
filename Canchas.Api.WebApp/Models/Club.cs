using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Canchas.Api.WebApp.Models
{
    public class Club
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = "";

        [MaxLength(200)]
        public string Direccion { get; set; } = "";

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(10)]
        public string? RegionCodigo { get; set; }  // RM, V, etc.

        [MaxLength(100)]
        public string? RegionNombre { get; set; }

        [MaxLength(10)]
        public string? ComunaCodigo { get; set; }

        [MaxLength(100)]
        public string? ComunaNombre { get; set; }

        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }

        [Column(TypeName = "JSON")]
        public List<string>? MetodosPagoHabilitados { get; set; } = new() { "Efectivo" };

        [Column(TypeName = "JSON")]
        public Dictionary<string, object>? ConfigPagos { get; set; }  // {Transbank: {CommerceCode, ApiKey}, MercadoPago: {AccessToken}}

        [Column(TypeName = "JSON")]
        public List<string>? AmenitiesJson { get; set; }

        [MaxLength(500)]
        public string? FotoPrincipalUrl { get; set; }

        public string? Descripcion { get; set; }

        [MaxLength(50)]
        public string Subdominio { get; set; } = "";

        public int? OwnerUserId { get; set; }
        public User? OwnerUser { get; set; } = null!;

        public EstadoSuscripcionClub EstadoSuscripcion { get; set; } = EstadoSuscripcionClub.PendientePago;
        public DateTime? FechaProxVencimiento { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        public List<Cancha> Canchas { get; set; } = [];
        public List<SuscripcionClub> Suscripciones { get; set; } = [];
    }
}
