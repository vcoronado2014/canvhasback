using Canchas.Api.WebApp.DTOS;
using Canchas.Api.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Canchas.Api.WebApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ClubController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClubController(AppDbContext context) => _context = context;

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("crear")]
        public async Task<IActionResult> CrearClub([FromBody] RegistroClubRequest request)
        {
            // 1. Validar subdominio
            if (await _context.Clubs.AnyAsync(c => c.Subdominio == request.Subdominio))
                return BadRequest(new { mensaje = "El subdominio ya está en uso." });

            try
            {
                // 2. Crear el Club
                var club = new Club
                {
                    Nombre = request.NombreClub,
                    Subdominio = request.Subdominio.ToLower().Trim(),
                    Direccion = request.Direccion,
                    Telefono = request.Telefono,
                    Descripcion = request.Descripcion,
                    RegionCodigo = request.RegionCodigo,
                    RegionNombre = request.RegionNombre,
                    ComunaCodigo = request.ComunaCodigo,
                    ComunaNombre = request.ComunaNombre,
                    MetodosPagoHabilitados = request.MetodosPagoHabilitados ?? new List<string> { "Efectivo" },
                    EstadoSuscripcion = request.EstadoSuscripcion,
                    FechaProxVencimiento = request.FechaProxVencimiento,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Clubs.Add(club);
                await _context.SaveChangesAsync();

                return Ok(new { ClubId = club.Id, mensaje = "Club creado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear el club.", detalle = ex.Message });
            }
        }

        [Authorize(Roles = "SuperAdmin,ClubAdmin,AgendaCreator,CourtManager")]
        [HttpGet]
        public async Task<IActionResult> ObtenerClubes()
        {
            // 1. Obtener el ID del usuario desde los Claims del Token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            // 2. Iniciar la consulta IQueryable
            var query = _context.Clubs.AsQueryable();

            // 3. Si NO es SuperAdmin, filtrar estrictamente por su ClubId
            if (!User.IsInRole("SuperAdmin"))
            {
                var userClubId = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.ClubId)
                    .FirstOrDefaultAsync();

                if (userClubId == null)
                {
                    return Ok(new List<object>());
                }

                query = query.Where(c => c.Id == userClubId.Value);
            }

            // 4. Proyectar y ejecutar la consulta
            var clubes = await query
                .Include(c => c.OwnerUser)
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    c.Subdominio,
                    c.Direccion,            // AGREGADO: útil para vistas resumidas o tablas
                    c.Telefono,             // AGREGADO: útil para vistas resumidas o tablas
                    c.EstadoSuscripcion,
                    c.FechaProxVencimiento,
                    c.RegionNombre,
                    c.ComunaNombre,
                    Owner = c.OwnerUser != null ? c.OwnerUser.Email : null // Seguro ante nulls
                })
                .ToListAsync();

            return Ok(clubes);
        }

        [Authorize(Roles = "SuperAdmin,ClubAdmin,AgendaCreator,CourtManager")]
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerClub(int id)
        {
            // Validación de seguridad para usuarios no SuperAdmin
            if (!User.IsInRole("SuperAdmin"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var userClubId = await _context.Users
                        .Where(u => u.Id == userId)
                        .Select(u => u.ClubId)
                        .FirstOrDefaultAsync();

                    if (userClubId != id)
                    {
                        return Forbid(); // Intenta acceder a otro club que no es el suyo
                    }
                }
            }

            var club = await _context.Clubs
                .Include(c => c.OwnerUser)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (club == null)
                return NotFound();

            return Ok(new
            {
                club.Id,
                club.Nombre,
                club.Direccion,
                club.Telefono,
                club.RegionCodigo,
                club.RegionNombre,
                club.ComunaCodigo,
                club.ComunaNombre,
                club.Latitud,
                club.Longitud,
                club.MetodosPagoHabilitados,
                club.ConfigPagos,
                club.AmenitiesJson,
                club.FotoPrincipalUrl,
                club.Descripcion,
                club.Subdominio,
                club.EstadoSuscripcion,
                club.FechaProxVencimiento,
                Owner = club.OwnerUser != null ? new
                {
                    club.OwnerUser.Id,
                    club.OwnerUser.Email
                } : null
            });
        }

        [Authorize(Roles = "SuperAdmin,ClubAdmin,AgendaCreator,CourtManager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarClub(int id, [FromBody] ActualizarClubRequest request)
        {
            var club = await _context.Clubs.FindAsync(id);
            if (club == null)
                return NotFound(new { mensaje = "El club no existe." });

            bool isSuperAdmin = User.IsInRole("SuperAdmin");

            // Si NO es SuperAdmin, validar que solo edite su propio club
            if (!isSuperAdmin)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized();

                var userClubId = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.ClubId)
                    .FirstOrDefaultAsync();

                if (userClubId != id)
                    return Forbid();

                // Rol común: SOLO puede actualizar Teléfono y Descripción
                club.Telefono = request.Telefono;
                club.Descripcion = request.Descripcion;
            }
            else
            {
                // SuperAdmin: Puede actualizar TODOS los campos
                if (await _context.Clubs.AnyAsync(c => c.Subdominio == request.Subdominio && c.Id != id))
                    return BadRequest(new { mensaje = "El subdominio ya está en uso por otro club." });

                club.Nombre = request.NombreClub;
                club.Subdominio = request.Subdominio.ToLower().Trim();
                club.Direccion = request.Direccion;
                club.Telefono = request.Telefono;
                club.Descripcion = request.Descripcion;
                club.RegionCodigo = request.RegionCodigo;
                club.RegionNombre = request.RegionNombre;
                club.ComunaCodigo = request.ComunaCodigo;
                club.ComunaNombre = request.ComunaNombre;
                if (request.MetodosPagoHabilitados != null)
                    club.MetodosPagoHabilitados = request.MetodosPagoHabilitados;
                club.EstadoSuscripcion = request.EstadoSuscripcion;
                club.FechaProxVencimiento = request.FechaProxVencimiento;
            }

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Club actualizado correctamente." });
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarClub(int id)
        {
            var club = await _context.Clubs.FindAsync(id);
            if (club == null)
                return NotFound(new { mensaje = "El club no existe." });

            // Opcional: Validar que no tenga datos dependientes críticos antes de borrar
            _context.Clubs.Remove(club);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Club eliminado correctamente." });
        }
    }
}
