using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Canchas.Api.WebApp.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Canchas.Api.WebApp.Models;
using Canchas.Api.WebApp.Services;

namespace Canchas.Api.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservasController : ControllerBase
    {
        private readonly IReservaService _reservaService;
        private readonly AppDbContext _context;

        public ReservasController(IReservaService reservaService, AppDbContext context)
        {
            _reservaService = reservaService;
            _context = context;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        private bool IsSuperAdmin() => User.IsInRole("SuperAdmin");
        private int? GetCurrentClubId()
        {
            var clubIdClaim = User.FindFirst("ClubId")?.Value;
            return int.TryParse(clubIdClaim, out var id) ? id : null;
        }

        // ----------------------------------------------------------------------
        // PUBLICO (Landing Page): Ver la grilla de oferta/disponibilidad
        // ----------------------------------------------------------------------
        // GET: api/reservas/disponibilidad?comuna=Puente%20Alto&lat=-33.6&lon=-70.5
        [HttpGet("disponibilidad")]
        [AllowAnonymous] // Accesso libre para clientes sin loguear desde la Landing
        public async Task<ActionResult<List<CanchaOfertaDto>>> GetDisponibilidad(
            [FromQuery] int? clubId,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] double? lat = null,
            [FromQuery] double? lon = null,
            [FromQuery] double? radiusKm = 10.0,
            [FromQuery] string? comuna = null,
            [FromQuery] string? region = null)
        {
            var start = fechaInicio ?? DateTime.UtcNow.Date;
            var end = fechaFin ?? start.AddDays(1);

            // 1. Consulta directa por Club (Panel de Staff o Vista Detalle de Club)
            if (clubId.HasValue)
            {
                var oferta = await _reservaService.ConsultarDisponibilidadClubAsync(clubId.Value, start, end);
                return Ok(oferta);
            }

            // 2. Búsqueda con filtros de la Landing Page
            List<Club> clubesEncontrados = new List<Club>();

            var query = _context.Clubs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(region))
            {
                query = query.Where(c => c.RegionCodigo == region);
            }

            if (!string.IsNullOrWhiteSpace(comuna))
            {
                query = query.Where(c => c.ComunaCodigo == comuna);
            }

            if (!string.IsNullOrWhiteSpace(comuna) || !string.IsNullOrWhiteSpace(region))
            {
                clubesEncontrados = await query.ToListAsync();
            }
            else if (lat.HasValue && lon.HasValue)
            {
                // Búsqueda por Radio GPS
                var todosConCoords = await _context.Clubs
                    .Where(c => c.Latitud.HasValue && c.Longitud.HasValue)
                    .ToListAsync();

                foreach (var club in todosConCoords)
                {
                    var d = DistanceKmBetweenPoints((double)club.Latitud!.Value, (double)club.Longitud!.Value, lat.Value, lon.Value);
                    if (d <= (radiusKm ?? 10.0)) clubesEncontrados.Add(club);
                }
            }
            else
            {
                // SIN FILTROS: Fallback por omisión (Primeros 10 clubes)
                clubesEncontrados = await _context.Clubs.Take(10).ToListAsync();
            }

            var resultadoAgregado = new List<CanchaOfertaDto>();
            foreach (var club in clubesEncontrados)
            {
                var ofertaClub = await _reservaService.ConsultarDisponibilidadClubAsync(club.Id, start, end);
                resultadoAgregado.AddRange(ofertaClub);
            }

            return Ok(resultadoAgregado);
        }

        // ----------------------------------------------------------------------
        // CLIENTE: Mis Reservas Personales
        // ----------------------------------------------------------------------
        // GET: api/reservas/mis-reservas
        [HttpGet("mis-reservas")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<List<ReservaReadDto>>> GetMisReservas()
        {
            var clienteUserId = GetUserId();
            var reservas = await _reservaService.ObtenerReservasPorClienteAsync(clienteUserId);
            return Ok(reservas);
        }

        // ----------------------------------------------------------------------
        // STAFF / ADMIN: Ver todas las reservas del club
        // ----------------------------------------------------------------------
        // GET: api/reservas?clubId=1&fechaInicio=2026-08-01
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,ClubAdmin,CourtManager,AgendaCreator")]
        public async Task<ActionResult<List<ReservaReadDto>>> GetReservasClub(
            [FromQuery] int? clubId = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            int targetClubId;

            if (IsSuperAdmin())
            {
                if (!clubId.HasValue) return BadRequest("El parámetro clubId es requerido para SuperAdmin.");
                targetClubId = clubId.Value;
            }
            else
            {
                var userClubId = GetCurrentClubId();
                if (!userClubId.HasValue) return BadRequest("El usuario no tiene un club asignado.");
                targetClubId = userClubId.Value;
            }

            var reservas = await _reservaService.ObtenerReservasClubAsync(targetClubId, fechaInicio, fechaFin);
            return Ok(reservas);
        }

        // ----------------------------------------------------------------------
        // CREACIÓN DE RESERVAS (Cliente Online vs Staff Presencial)
        // ----------------------------------------------------------------------
        // POST: api/reservas/online (Cliente desde la app/web)
        //[HttpPost("online")]
        //[Authorize(Roles = "Cliente")]
        //public async Task<ActionResult<ReservaReadDto>> CrearReservaCliente([FromBody] CrearReservaClienteDto dto)
        //{
        //    try
        //    {
        //        var userId = GetUserId();
        //        var reserva = await _reservaService.CrearReservaClienteAsync(dto, userId);
        //        return CreatedAtAction(nameof(GetMisReservas), new { id = reserva.Id }, reserva);
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

        [HttpPost("online")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<List<ReservaReadDto>>> CrearReservaCliente([FromBody] CrearReservaClienteLoteDto dto)
        {
            var userId = GetUserId();
            var reservasCreadas = new List<Reserva>();

            foreach (var bloque in dto.Bloques)
            {
                var reserva = new Reserva
                {
                    CanchaId = dto.CanchaId,
                    ClienteId = userId,
                    FechaInicio = bloque.FechaInicio,
                    FechaFin = bloque.FechaFin,
                    MontoTotal = dto.MontoTotal,
                    MetodoPago = dto.MetodoPago,
                    Estado = EstadoReserva.Confirmada,
                    CreatedByUserId = userId,
                    FechaCreacion = DateTime.UtcNow,
                    MontoPagado = dto.MontoTotal
                };
                reservasCreadas.Add(reserva);
            }

            _context.Reservas.AddRange(reservasCreadas);
            await _context.SaveChangesAsync();

            return Ok(reservasCreadas);
        }


        // POST: api/reservas/presencial (Caja/Recepción)
        [HttpPost("presencial")]
        [Authorize(Roles = "SuperAdmin,ClubAdmin,CourtManager,AgendaCreator")]
        public async Task<ActionResult<ReservaReadDto>> CrearReservaPresencial([FromBody] CrearReservaPresencialDto dto)
        {
            try
            {
                var userId = GetUserId();
                var reserva = await _reservaService.CrearReservaPresencialAsync(dto, userId);
                return Ok(reserva);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ----------------------------------------------------------------------
        // CANCELACIÓN DE RESERVAS
        // ----------------------------------------------------------------------
        // DELETE: api/reservas/5/cancelar
        [HttpDelete("{id}/cancelar")]
        [Authorize] // Tanto el Cliente (dueño) como el Staff pueden solicitar cancelar
        public async Task<IActionResult> CancelarReserva(int id)
        {
            try
            {
                var userId = GetUserId();
                var isSuperAdmin = IsSuperAdmin();
                var clubId = GetCurrentClubId();

                await _reservaService.CancelarReservaAsync(id, userId, isSuperAdmin, clubId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("La reserva no fue encontrada.");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Métodos de apoyo de geolocalización
        private static double DegreesToRadians(double deg) => deg * Math.PI / 180.0;
        private static double DistanceKmBetweenPoints(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371;
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}