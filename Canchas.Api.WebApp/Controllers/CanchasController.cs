using Canchas.Api.WebApp.DTOS;
using Canchas.Api.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Canchas.Api.WebApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CanchasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CanchasController(AppDbContext context) => _context = context;

        [Authorize(Roles = "SuperAdmin,ClubAdmin,CourtManager")]
        [HttpPost]
        public async Task<IActionResult> CrearCancha([FromBody] CrearCanchaRequest request)
        {
            // 1. Validar que el club exista
            var clubExiste = await _context.Clubs.AnyAsync(c => c.Id == request.ClubId);
            if (!clubExiste)
                return BadRequest("El club especificado no existe.");

            try
            {
                var cancha = new Cancha
                {
                    ClubId = request.ClubId,
                    Nombre = request.Nombre,
                    TipoCancha = request.TipoCancha,
                    PrecioHora = request.PrecioHora,
                    HorarioInicio = request.HorarioInicio,
                    HorarioFin = request.HorarioFin,
                    DuracionMinimaMinutos = request.DuracionMinimaMinutos,
                    Activa = request.Activa
                };

                _context.Canchas.Add(cancha);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(ObtenerCancha), new { id = cancha.Id }, new { cancha.Id });
            }
            catch
            {
                return StatusCode(500, "Error al registrar la cancha.");
            }
        }

        [Authorize(Roles = "SuperAdmin,ClubAdmin,AgendaCreator,CourtManager")]
        [HttpGet("club/{clubId}")]
        public async Task<IActionResult> ObtenerCanchasPorClub(int clubId)
        {
            var canchas = await _context.Canchas
                .Where(c => c.ClubId == clubId)
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    c.Id,
                    c.ClubId,
                    c.Nombre,
                    c.TipoCancha,
                    c.PrecioHora,
                    c.HorarioInicio,
                    c.HorarioFin,
                    c.DuracionMinimaMinutos,
                    c.Activa,
                    Fotos = c.Fotos.OrderBy(f => f.Orden).Select(f => new
                    {
                        f.Id,
                        f.Url,
                        f.EsPrincipal,
                        f.Orden
                    }),
                    HorariosTarifasCount = c.HorariosTarifas.Count,
                    BloqueosActivosCount = c.Bloqueos.Count(b => b.FechaFin >= DateTime.UtcNow)
                })
                .ToListAsync();

            return Ok(canchas);
        }

        [Authorize(Roles = "SuperAdmin,ClubAdmin,AgendaCreator,CourtManager")]
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerCancha(int id)
        {
            var cancha = await _context.Canchas
                .Include(c => c.Fotos)
                .Include(c => c.HorariosTarifas)
                .Include(c => c.Bloqueos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cancha == null)
                return NotFound("La cancha no existe.");

            return Ok(new
            {
                cancha.Id,
                cancha.ClubId,
                cancha.Nombre,
                cancha.TipoCancha,
                cancha.PrecioHora,
                cancha.HorarioInicio,
                cancha.HorarioFin,
                cancha.DuracionMinimaMinutos,
                cancha.Activa,
                Fotos = cancha.Fotos.OrderBy(f => f.Orden).Select(f => new
                {
                    f.Id,
                    f.Url,
                    f.EsPrincipal,
                    f.Orden
                }),
                HorariosTarifas = cancha.HorariosTarifas.Select(h => new
                {
                    h.Id,
                    h.DiaSemana,
                    h.HoraInicio,
                    h.HoraFin,
                    h.PrecioPorBloque
                }),
                Bloqueos = cancha.Bloqueos.Select(b => new
                {
                    b.Id,
                    b.FechaInicio,
                    b.FechaFin,
                    b.Motivo
                })
            });
        }

        [Authorize(Roles = "SuperAdmin,ClubAdmin,CourtManager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCancha(int id, [FromBody] ActualizarCanchaRequest request)
        {
            var cancha = await _context.Canchas.FindAsync(id);
            if (cancha == null)
                return NotFound("La cancha no existe.");

            try
            {
                cancha.Nombre = request.Nombre;
                cancha.TipoCancha = request.TipoCancha;
                cancha.PrecioHora = request.PrecioHora;
                cancha.HorarioInicio = request.HorarioInicio;
                cancha.HorarioFin = request.HorarioFin;
                cancha.DuracionMinimaMinutos = request.DuracionMinimaMinutos;
                cancha.Activa = request.Activa;

                await _context.SaveChangesAsync();
                return Ok(new { Mensaje = "Cancha actualizada exitosamente.", cancha.Id });
            }
            catch
            {
                return StatusCode(500, "Error al actualizar la cancha.");
            }
        }

        [Authorize(Roles = "SuperAdmin,ClubAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCancha(int id)
        {
            var cancha = await _context.Canchas
                .Include(c => c.Reservas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cancha == null)
                return NotFound("La cancha no existe.");

            try
            {
                // Si la cancha tiene historial de reservas, hacemos soft-delete
                if (cancha.Reservas.Any())
                {
                    cancha.Activa = false;
                    await _context.SaveChangesAsync();
                    return Ok(new { Mensaje = "La cancha tiene reservas asociadas. Se ha desactivado (soft-delete)." });
                }

                // Si no tiene reservas, se puede eliminar físicamente
                _context.Canchas.Remove(cancha);
                await _context.SaveChangesAsync();

                return Ok(new { Mensaje = "Cancha eliminada exitosamente." });
            }
            catch
            {
                return StatusCode(500, "Error al eliminar la cancha.");
            }
        }
    }
}
