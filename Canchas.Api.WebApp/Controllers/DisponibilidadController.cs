using Canchas.Api.WebApp.DTOS;
using Canchas.Api.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Canchas.Api.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,ClubAdmin,AgendaCreator,CourtManager")]
    public class DisponibilidadController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DisponibilidadController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsSuperAdmin() => User.IsInRole("SuperAdmin");

        private int? GetCurrentClubId()
        {
            var clubIdClaim = User.FindFirst("ClubId")?.Value;
            return int.TryParse(clubIdClaim, out var clubId) ? clubId : null;
        }

        // POST: api/disponibilidad/rango
        // Permite generar disponibilidad masiva (ej: del 1 al 31 de julio)
        [HttpPost("rango")]
        public async Task<IActionResult> CrearDisponibilidadRango([FromBody] CrearDisponibilidadRangoDto dto)
        {
            var cancha = await _context.Canchas.Include(c => c.Club).FirstOrDefaultAsync(c => c.Id == dto.CanchaId);
            if (cancha == null) return NotFound("La cancha especificada no existe.");

            if (!IsSuperAdmin())
            {
                var userClubId = GetCurrentClubId();
                if (!userClubId.HasValue || userClubId.Value != cancha.ClubId)
                    return Forbid();
            }

            var disponibilidadesNuevas = new List<Disponibilidad>();

            // Iterar desde FechaDesde hasta FechaHasta
            for (var fecha = dto.FechaDesde.Date; fecha <= dto.FechaHasta.Date; fecha = fecha.AddDays(1))
            {
                // Verificar que no exista ya un registro exacto para esa cancha y fecha/hora
                bool existe = await _context.Disponibilidades.AnyAsync(d =>
                    d.CanchaId == dto.CanchaId &&
                    d.Fecha == fecha &&
                    d.HoraInicio == dto.HoraInicio &&
                    d.HoraFin == dto.HoraFin);

                if (!existe)
                {
                    disponibilidadesNuevas.Add(new Disponibilidad
                    {
                        CanchaId = dto.CanchaId,
                        Fecha = fecha,
                        HoraInicio = dto.HoraInicio,
                        HoraFin = dto.HoraFin,
                        Motivo = dto.Motivo
                    });
                }
            }

            if (disponibilidadesNuevas.Count == 0)
            {
                return BadRequest("No se crearon disponibilidades. Ya existían o el rango de fechas es inválido.");
            }

            _context.Disponibilidades.AddRange(disponibilidadesNuevas);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Se crearon {disponibilidadesNuevas.Count} bloques de disponibilidad exitosamente." });
        }

        // GET: api/disponibilidad/club/{clubId}?fecha=2026-08-01
        //[HttpGet("club/{clubId}")]
        //public async Task<IActionResult> ListarDisponibilidadesPorClub(int clubId, [FromQuery] DateTime? fecha = null)
        //{
        //    if (!IsSuperAdmin() && GetCurrentClubId() != clubId)
        //        return Forbid();

        //    var inicio = fecha?.Date ?? DateTime.UtcNow.Date;

        //    var list = await _context.Disponibilidades
        //        .Include(d => d.Cancha)
        //        .Where(d => d.Cancha.ClubId == clubId && d.Fecha >= inicio)
        //        .OrderBy(d => d.Fecha).ThenBy(d => d.HoraInicio)
        //        .ToListAsync();

        //    return Ok(list);
        //}

        // GET: api/disponibilidad/club/{clubId}?fecha=2026-08-01
        [HttpGet("club/{clubId}")]
        public async Task<IActionResult> ListarDisponibilidadesPorClub(int clubId, [FromQuery] DateTime? fecha = null)
        {
            if (!IsSuperAdmin() && GetCurrentClubId() != clubId)
                return Forbid();

            var inicio = fecha?.Date ?? DateTime.UtcNow.Date;

            var list = await _context.Disponibilidades
                .Where(d => d.Cancha.ClubId == clubId && d.Fecha >= inicio)
                .OrderBy(d => d.Fecha)
                .ThenBy(d => d.HoraInicio)
                .Select(d => new DisponibilidadReadDto
                {
                    Id = d.Id,
                    CanchaId = d.CanchaId,
                    NombreCancha = d.Cancha.Nombre,
                    Fecha = d.Fecha,
                    HoraInicio = d.HoraInicio,
                    HoraFin = d.HoraFin,
                    Motivo = d.Motivo
                })
                .ToListAsync();

            return Ok(list);
        }

        // DELETE: api/disponibilidad/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarDisponibilidad(int id)
        {
            var disp = await _context.Disponibilidades.Include(d => d.Cancha).FirstOrDefaultAsync(d => d.Id == id);
            if (disp == null) return NotFound();

            if (!IsSuperAdmin())
            {
                var userClubId = GetCurrentClubId();
                if (!userClubId.HasValue || userClubId.Value != disp.Cancha.ClubId)
                    return Forbid();
            }

            // Validar que no haya reservas activas asociadas a esta disponibilidad específica
            var tieneReservas = await _context.Reservas.AnyAsync(r =>
                r.CanchaId == disp.CanchaId &&
                r.FechaInicio.Date == disp.Fecha &&
                r.Estado != EstadoReserva.Cancelada);

            if (tieneReservas)
            {
                return BadRequest("No se puede eliminar la disponibilidad porque existen reservas asociadas activas. Cancele las reservas primero.");
            }

            _context.Disponibilidades.Remove(disp);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
