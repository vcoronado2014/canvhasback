using Canchas.Api.WebApp.DTOS;
using Canchas.Api.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Canchas.Api.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservasController : ControllerBase
    {
        private readonly IReservaService _reservaService;

        public ReservasController(IReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        private bool IsSuperAdmin() => User.IsInRole("SuperAdmin");

        private int? GetCurrentClubId()
        {
            var clubIdClaim = User.FindFirst("ClubId")?.Value;
            return int.TryParse(clubIdClaim, out var id) ? id : null;
        }

        // GET: api/reservas/disponibilidad?clubId=1&fecha=2026-08-01
        [HttpGet("disponibilidad")]
        public async Task<ActionResult<List<CanchaOfertaDto>>> GetDisponibilidad(
            [FromQuery] int clubId,
            [FromQuery] DateTime fecha)
        {
            // Un Admin de Club solo puede consultar su propio club (salvo SuperAdmin)
            if (!IsSuperAdmin() && GetCurrentClubId() != clubId)
            {
                return Forbid();
            }

            var oferta = await _reservaService.ConsultarDisponibilidadClubAsync(clubId, fecha);
            return Ok(oferta);
        }

        // GET: api/reservas?clubId=1&fecha=2026-08-01
        [HttpGet]
        public async Task<ActionResult<List<ReservaReadDto>>> GetReservas(
            [FromQuery] int? clubId = null,
            [FromQuery] DateTime? fecha = null)
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

            var reservas = await _reservaService.ObtenerReservasClubAsync(targetClubId, fecha);
            return Ok(reservas);
        }

        // POST: api/reservas/presencial
        [HttpPost("presencial")]
        [Authorize(Roles = "SuperAdmin,ClubAdmin,CourtManager,AgendaCreator")]
        public async Task<ActionResult<ReservaReadDto>> CrearReservaPresencial([FromBody] CrearReservaPresencialDto dto)
        {
            try
            {
                var userId = GetUserId();
                var reserva = await _reservaService.CrearReservaPresencialAsync(dto, userId);
                return CreatedAtAction(nameof(GetReservas), new { id = reserva.Id }, reserva);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/reservas/5/cancelar
        [HttpDelete("{id}/cancelar")]
        [Authorize(Roles = "SuperAdmin,ClubAdmin,CourtManager")]
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
        }
    }
}
