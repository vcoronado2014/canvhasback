using System.ComponentModel.DataAnnotations;

namespace Canchas.Api.WebApp.Models
{
    public class CanchaFoto
    {
        public int Id { get; set; }
        public int CanchaId { get; set; }
        public Cancha Cancha { get; set; } = null!;

        [Required, MaxLength(500)]
        public string Url { get; set; } = "";

        public int Orden { get; set; } = 0; // Para ordenar las fotos en el carrusel
        public bool EsPrincipal { get; set; } = false;
    }
}
