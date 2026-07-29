namespace Canchas.Api.WebApp.Models
{
    public class CanchaHorarioTarifa
    {
        public int Id { get; set; }
        public int CanchaId { get; set; }
        public Cancha Cancha { get; set; } = null!;

        public DayOfWeek DiaSemana { get; set; } // Lunes, Martes, etc.
        public TimeSpan HoraInicio { get; set; }  // ej: 08:00
        public TimeSpan HoraFin { get; set; }     // ej: 18:00

        public decimal PrecioPorBloque { get; set; } // Precio por el intervalo de la cancha
    }
}
