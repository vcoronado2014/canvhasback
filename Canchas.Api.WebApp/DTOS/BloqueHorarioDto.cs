namespace Canchas.Api.WebApp.DTOS
{
    public class BloqueHorarioDto
    {
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public decimal Precio { get; set; }
        public bool Disponible { get; set; }
        public string? MotivoIndisponibilidad { get; set; } // "Reservado", "Mantenimiento", etc.
    }
}
