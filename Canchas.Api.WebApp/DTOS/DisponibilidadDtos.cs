using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.WebApp.DTOS
{
    public class CrearDisponibilidadRangoDto
    {
        [Required(ErrorMessage = "La cancha es requerida.")]
        public int CanchaId { get; set; }

        [Required(ErrorMessage = "La fecha desde es requerida.")]
        public DateTime FechaDesde { get; set; }

        [Required(ErrorMessage = "La fecha hasta es requerida.")]
        public DateTime FechaHasta { get; set; }

        [Required(ErrorMessage = "La hora de inicio es requerida.")]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "La hora de fin es requerida.")]
        public TimeSpan HoraFin { get; set; }

        /// <summary>
        /// Opcional: Motivo o etiqueta explicativa (ej: "Horario Hábil", "Mantenimiento", "Turno Noche")
        /// </summary>
        public string? Motivo { get; set; }

        /// <summary>
        /// Opcional: Días de la semana específicos a incluir (0=Domingo, 1=Lunes, ..., 6=Sábado).
        /// Si se envía vacío o null, aplica para todos los días dentro del rango.
        /// </summary>
        public List<DayOfWeek>? DiasSemana { get; set; }
    }

    public class CrearDisponibilidadDto
    {
        [Required]
        public int CanchaId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFin { get; set; }

        public string? Motivo { get; set; }
    }

    public class ActualizarDisponibilidadDto
    {
        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFin { get; set; }

        public string? Motivo { get; set; }
    }

    public class DisponibilidadReadDto
    {
        public int Id { get; set; }
        public int CanchaId { get; set; }
        public string NombreCancha { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string? Motivo { get; set; }
    }
}
