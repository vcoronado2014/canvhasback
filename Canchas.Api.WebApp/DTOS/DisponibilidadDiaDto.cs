namespace Canchas.Api.WebApp.DTOS
{
    public class DisponibilidadDiaDto
    {
        public int CanchaId { get; set; }
        public DateTime Fecha { get; set; }
        public List<BloqueHorarioDto> Bloques { get; set; } = new();
    }
}
