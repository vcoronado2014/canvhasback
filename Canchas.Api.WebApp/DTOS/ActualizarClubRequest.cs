using Canchas.Api.WebApp.Models;

namespace Canchas.Api.WebApp.DTOS
{
    public class ActualizarClubRequest
    {
        public string NombreClub { get; set; } = string.Empty;
        public string Subdominio { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Descripcion { get; set; }

        public string? RegionCodigo { get; set; }
        public string? RegionNombre { get; set; }
        public string? ComunaCodigo { get; set; }
        public string? ComunaNombre { get; set; }

        public List<string>? MetodosPagoHabilitados { get; set; }
        public EstadoSuscripcionClub EstadoSuscripcion { get; set; }
        public DateTime? FechaProxVencimiento { get; set; }
    }
}
