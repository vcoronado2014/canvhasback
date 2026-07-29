namespace Canchas.Api.WebApp.Models
{
    public class CanchaBloqueo
    {
        public int Id { get; set; }
        public int CanchaId { get; set; }
        public Cancha Cancha { get; set; } = null!;

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string? Motivo { get; set; } // Ej: "Mantenimiento césped"
    }
}
