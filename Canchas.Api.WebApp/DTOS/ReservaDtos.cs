using Canchas.Api.WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.WebApp.DTOS
{
    public class ConsultarDisponibilidadRequestDto
    {
        [Required]
        public int ClubId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }
    }

    public class SlotDisponibilidadDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal Precio { get; set; }
        public bool Disponible { get; set; }
        public string? MotivoOcupado { get; set; }
    }

    public class CanchaOfertaDto
    {
        // Datos del Club
        public int ClubId { get; set; }
        public string NombreClub { get; set; } = string.Empty;
        public string DireccionClub { get; set; } = string.Empty;
        public string ComunaNombre { get; set; } = string.Empty;
        public string RegionNombre { get; set; } = string.Empty;
        public string? FotoClubUrl { get; set; }

        // Datos de la Cancha
        public int CanchaId { get; set; }
        public string NombreCancha { get; set; } = string.Empty;
        public TipoCancha TipoCancha { get; set; }
        public decimal PrecioHoraBase { get; set; }
        public string? FotoPrincipalUrl { get; set; }
        public int DuracionMinimaMinutos { get; set; }

        // Slots
        public List<SlotDisponibilidadDto> HorariosDisponibles { get; set; } = new();
    }

    // DTO que faltaba para la Reserva Online del Cliente
    public class CrearReservaClienteDto
    {
        [Required]
        public int CanchaId { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public decimal MontoTotal { get; set; }

        public MetodoPago MetodoPago { get; set; }
    }

    public class CrearReservaPresencialDto
    {
        [Required]
        public int CanchaId { get; set; }

        public int? ClienteId { get; set; }
        public string? NombreClienteManual { get; set; }
        public string? TelefonoClienteManual { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public decimal MontoTotal { get; set; }

        public bool Pagado { get; set; } = false;
        public MetodoPago? MetodoPago { get; set; }
    }

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

    public class CrearReservaClienteLoteDto
    {
        public int CanchaId { get; set; }
        public List<TramoReservaDto> Bloques { get; set; } = new();
        public decimal MontoTotal { get; set; }
        public MetodoPago MetodoPago { get; set; }
    }

    public class TramoReservaDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
