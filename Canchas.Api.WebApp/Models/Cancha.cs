using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.WebApp.Models
{
    public class Cancha
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public Club Club { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = "";

        public TipoCancha TipoCancha { get; set; } = TipoCancha.Otro;
        public decimal PrecioHora { get; set; }
        public TimeSpan? HorarioInicio { get; set; }
        public TimeSpan? HorarioFin { get; set; }
        public int DuracionMinimaMinutos { get; set; } = 60;

        // Propiedad para soft delete / deshabilitar
        public bool Activa { get; set; } = true;
        // Opcional: Si quieres permitir bloques de 30m pero exigir mínimo 60m
        public int IntervaloMinutos { get; set; } = 30;

        public List<Disponibilidad> Disponibilidades { get; set; } = [];
        public List<Reserva> Reservas { get; set; } = [];
        //agregadas
        // Relaciones agregadas
        public List<CanchaFoto> Fotos { get; set; } = [];
        public List<CanchaHorarioTarifa> HorariosTarifas { get; set; } = [];
        public List<CanchaBloqueo> Bloqueos { get; set; } = [];

    }
}
