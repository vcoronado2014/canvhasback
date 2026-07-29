using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.WebApp.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public int CanchaId { get; set; }
        public Cancha Cancha { get; set; } = null!;

        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public decimal MontoTotal { get; set; }
        public MetodoPago? MetodoPago { get; set; }
        public decimal? MontoPagado { get; set; }
        public DateTime? FechaPagoReal { get; set; }

        [MaxLength(100)]
        public string? TransbankToken { get; set; }

        [MaxLength(50)]
        public string? TransbankBuyOrder { get; set; }

        [MaxLength(100)]
        public string? MercadoPagoPaymentId { get; set; }

        public EstadoReserva Estado { get; set; } = EstadoReserva.Pendiente;

        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;

        public List<PagoLog> PagoLogs { get; set; } = [];
    }
}
