using Canchas.Api.WebApp.Models;

namespace Canchas.Api.WebApp.DTOS
{
    public class ClubDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string? Telefono { get; set; }
        public string? RegionCodigo { get; set; }
        public string? RegionNombre { get; set; }
        public string? ComunaCodigo { get; set; }
        public string? ComunaNombre { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public string? FotoPrincipalUrl { get; set; }
        public string? Descripcion { get; set; }
        public string Subdominio { get; set; } = "";
        public EstadoSuscripcionClub EstadoSuscripcion { get; set; }
        public DateTime? FechaProxVencimiento { get; set; }
    }
}
