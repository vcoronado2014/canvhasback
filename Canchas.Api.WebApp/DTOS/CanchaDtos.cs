using Canchas.Api.WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.WebApp.DTOS
{
    public class CrearCanchaRequest
    {
        [Required]
        public int ClubId { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = "";

        public TipoCancha TipoCancha { get; set; } = TipoCancha.Otro;

        public decimal PrecioHora { get; set; }
        public TimeSpan? HorarioInicio { get; set; }
        public TimeSpan? HorarioFin { get; set; }
        public int DuracionMinimaMinutos { get; set; } = 60;
        public bool Activa { get; set; } = true;
    }

    public class ActualizarCanchaRequest
    {
        [Required, MaxLength(100)]
        public string Nombre { get; set; } = "";

        public TipoCancha TipoCancha { get; set; }
        public decimal PrecioHora { get; set; }
        public TimeSpan? HorarioInicio { get; set; }
        public TimeSpan? HorarioFin { get; set; }
        public int DuracionMinimaMinutos { get; set; }
        public bool Activa { get; set; }
    }
}
