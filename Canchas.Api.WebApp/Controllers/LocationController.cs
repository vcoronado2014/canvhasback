using Canchas.Api.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Canchas.Api.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LocationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocationController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/location/regiones
        // Devuelve todas las regiones de Chile
        [HttpGet("regiones")]
        public async Task<ActionResult<IEnumerable<RegionChile>>> GetRegiones()
        {
            return await _context.Regiones
                .OrderBy(r => r.Codigo == "RM" ? 0 : 1) // Tip: La RM siempre arriba para ahorrar scroll
                .ThenBy(r => r.Nombre)
                .ToListAsync();
        }

        // 2. GET: api/location/comunas/{regionCodigo}
        // Ejemplo: api/location/comunas/RM o api/location/comunas/05
        [HttpGet("comunas/{regionCodigo}")]
        public async Task<ActionResult<IEnumerable<ComunaChile>>> GetComunas(string regionCodigo)
        {
            var comunas = await _context.ComunasChile
                .Where(c => c.RegionCodigo == regionCodigo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            if (comunas == null || !comunas.Any())
            {
                return NotFound(new { mensaje = "No se encontraron comunas para el código de región proporcionado." });
            }

            return Ok(comunas);
        }

        // 3. GET: api/location/comuna/{codigo}
        // Obtiene el detalle de una sola comuna (ej: para mostrar el nombre en un perfil)
        [HttpGet("comuna/{codigo}")]
        public async Task<ActionResult<ComunaChile>> GetComunaByCodigo(string codigo)
        {
            var comuna = await _context.ComunasChile
                .Include(c => c.Region)
                .FirstOrDefaultAsync(c => c.Codigo == codigo);

            if (comuna == null) return NotFound();

            return Ok(comuna);
        }
    }
}
