namespace Canchas.Api.WebApp.Models
{
    public class SuscripcionClub
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public Club Club { get; set; } = null!;

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal MontoPagado { get; set; }
        public MetodoPago MetodoPagoSuscripcion { get; set; }
        public EstadoSuscripcionClub Estado { get; set; }
    }
}
