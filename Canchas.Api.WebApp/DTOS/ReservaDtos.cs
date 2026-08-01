using Canchas.Api.WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.WebApp.DTOS
{
    // DTO para consultar la oferta de canchas de un club en una fecha dada
    public class ConsultarDisponibilidadRequestDto
    {
        [Required]
        public int ClubId { get; set; }

        [Required]
        public DateTime Fecha { get; set; } // Formato YYYY-MM-DD
    }

    public class SlotDisponibilidadDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal Precio { get; set; }
        public bool Disponible { get; set; }
        public string? MotivoOcupado { get; set; } // "Reservada", "Bloqueada por mantenimiento", etc.
    }

    public class CanchaOfertaDto
    {
        public int CanchaId { get; set; }
        public string NombreCancha { get; set; } = string.Empty;
        public TipoCancha TipoCancha { get; set; }
        public decimal PrecioHoraBase { get; set; }
        public string? FotoPrincipalUrl { get; set; }
        public List<SlotDisponibilidadDto> HorariosDisponibles { get; set; } = new List<SlotDisponibilidadDto>();
    }

    // DTO para crear una reserva presencial
    public class CrearReservaPresencialDto
    {
        [Required]
        public int CanchaId { get; set; }

        public int? ClienteId { get; set; }

        // Si no está registrado como Cliente formal
        public string? NombreClienteManual { get; set; }
        public string? TelefonoClienteManual { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public decimal MontoTotal { get; set; }

        // Si paga en el acto al reservar presencialmente o si queda en "Pendiente" para pagar al llegar
        public bool Pagado { get; set; } = false;
        public MetodoPago? MetodoPago { get; set; }
    }

    // DTO de respuesta detallada de una reserva
    public class ReservaReadDto
    {
        public int Id { get; set; }
        public int CanchaId { get; set; }
        public string NombreCancha { get; set; } = string.Empty;
        public int? ClienteId { get; set; }
        public string? NombreCliente { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal MontoTotal { get; set; }
        public EstadoReserva Estado { get; set; }
        public MetodoPago? MetodoPago { get; set; }
        public decimal? MontoPagado { get; set; }
        public DateTime? FechaPagoReal { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreadoPorUsuario { get; set; } = string.Empty;
    }
}
