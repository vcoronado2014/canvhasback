namespace Canchas.Api.WebApp.Models
{
    public class Disponibilidad
    {
        public int Id { get; set; }
        public int CanchaId { get; set; }
        public Cancha Cancha { get; set; } = null!;

        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string? Motivo { get; set; }
    }
}
