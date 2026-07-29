using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Canchas.Api.WebApp.Models
{
    [Table("Regiones")]
    public class RegionChile
    {
        [Key]
        [MaxLength(10)]
        public string Codigo { get; set; } = null!; // Ejemplo: "RM", "05"

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = null!;

        // Relación: Una región tiene muchas comunas
        [JsonIgnore]
        public ICollection<ComunaChile> Comunas { get; set; } = new List<ComunaChile>();
    }

    [Table("ComunasChile")]
    public class ComunaChile
    {
        [Key]
        [MaxLength(10)]
        public string Codigo { get; set; } = null!; // Ejemplo: "13122"

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = null!;

        [Required]
        [MaxLength(10)]
        public string RegionCodigo { get; set; } = null!;

        [ForeignKey("RegionCodigo")]
        public RegionChile Region { get; set; } = null!;
    }
}
