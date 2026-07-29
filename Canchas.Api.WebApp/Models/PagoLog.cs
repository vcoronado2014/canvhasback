using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Canchas.Api.WebApp.Models
{
    public class PagoLog
    {
        public int Id { get; set; }
        public int ReservaId { get; set; }
        public Reserva Reserva { get; set; } = null!;

        [MaxLength(20)]
        public string Provider { get; set; } = "";

        [MaxLength(100)]
        public string PaymentId { get; set; } = "";

        public decimal Amount { get; set; }
        [MaxLength(50)]
        public string Status { get; set; } = "";

        [Column(TypeName = "JSON")]
        public object? ResponseData { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
