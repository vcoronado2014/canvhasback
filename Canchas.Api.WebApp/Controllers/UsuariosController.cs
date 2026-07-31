using Canchas.Api.WebApp.DTOS;
using Canchas.Api.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Canchas.Api.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Autorizado para usuarios autenticados
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // Helper para extraer el ClubId del token del usuario autenticado
        private int? GetCurrentClubId()
        {
            var clubIdClaim = User.FindFirst("ClubId")?.Value
                           ?? User.FindFirst("clubId")?.Value;

            return int.TryParse(clubIdClaim, out var clubId) ? clubId : null;
        }

        // Helper para verificar si es SuperAdmin
        private bool IsSuperAdmin() => User.IsInRole("SuperAdmin");

        // GET: api/usuarios
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,ClubAdmin,CourtManager,AgendaCreator")]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsuarios([FromQuery] int? clubId = null)
        {
            var query = _context.Users.Include(u => u.Club).AsQueryable();

            if (IsSuperAdmin())
            {
                // Si es SuperAdmin ve TODOS los usuarios de TODOS los clubes.
                // Si decide mandar un ?clubId=X en la URL, se le filtra opcionalmente.
                if (clubId.HasValue)
                {
                    query = query.Where(u => u.ClubId == clubId.Value);
                }
            }
            else
            {
                // Para CUALQUIER otro rol (ClubAdmin, CourtManager, etc.), 
                // se limita ESTRICTAMENTE al club al que pertenece.
                var userClubId = GetCurrentClubId();

                if (userClubId == null)
                {
                    return BadRequest("El usuario autenticado no tiene un club asignado.");
                }

                query = query.Where(u => u.ClubId == userClubId);
            }

            var usuarios = await query
                .Select(u => new UserReadDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Nombre = u.Nombre,
                    Telefono = u.Telefono,
                    Rol = u.Rol,
                    ClubId = u.ClubId,
                    NombreClub = u.Club != null ? u.Club.Nombre : null
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // GET: api/usuarios/5
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,ClubAdmin,CourtManager,AgendaCreator")]
        public async Task<ActionResult<UserReadDto>> GetUsuario(int id)
        {
            var user = await _context.Users.Include(u => u.Club).FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound("Usuario no encontrado.");

            // Si no es SuperAdmin, verificar que el usuario buscado pertenezca a su mismo club
            if (!IsSuperAdmin() && user.ClubId != GetCurrentClubId())
            {
                return Forbid();
            }

            return Ok(new UserReadDto
            {
                Id = user.Id,
                Email = user.Email,
                Nombre = user.Nombre,
                Telefono = user.Telefono,
                Rol = user.Rol,
                ClubId = user.ClubId,
                NombreClub = user.Club?.Nombre
            });
        }

        // POST: api/usuarios
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,ClubAdmin")]
        public async Task<ActionResult<UserReadDto>> CreateUsuario([FromBody] UserCreateDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            {
                return BadRequest("El correo electrónico ya está registrado.");
            }

            // Si es SuperAdmin, usa el ClubId enviado en el DTO (o null si es otro SuperAdmin).
            // Si es ClubAdmin, se le fuerza a crear el usuario en SU PROPIO club.
            if (!IsSuperAdmin())
            {
                if (dto.Rol == RolUsuario.SuperAdmin)
                {
                    return BadRequest("No tienes permisos para crear un SuperAdmin.");
                }

                dto.ClubId = GetCurrentClubId();
            }

            var user = new User
            {
                Email = dto.Email,
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Rol = dto.Rol,
                ClubId = dto.ClubId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuario), new { id = user.Id }, new UserReadDto
            {
                Id = user.Id,
                Email = user.Email,
                Nombre = user.Nombre,
                Telefono = user.Telefono,
                Rol = user.Rol,
                ClubId = user.ClubId
            });
        }

        // PUT: api/usuarios/5
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,ClubAdmin")]
        public async Task<IActionResult> UpdateUsuario(int id, [FromBody] UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Usuario no encontrado.");

            if (!IsSuperAdmin())
            {
                // Un ClubAdmin solo puede modificar usuarios de su club
                if (user.ClubId != GetCurrentClubId()) return Forbid();
                if (dto.Rol == RolUsuario.SuperAdmin) return BadRequest("No puedes asignar el rol SuperAdmin.");

                // Forzar a mantener el ClubId original
                dto.ClubId = user.ClubId;
            }

            if (user.Email.ToLower() != dto.Email.ToLower() &&
                await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            {
                return BadRequest("El correo electrónico ya está registrado por otro usuario.");
            }

            user.Email = dto.Email;
            user.Nombre = dto.Nombre;
            user.Telefono = dto.Telefono;
            user.Rol = dto.Rol;
            user.ClubId = dto.ClubId;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/usuarios/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,ClubAdmin")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Usuario no encontrado.");

            if (!IsSuperAdmin() && user.ClubId != GetCurrentClubId())
            {
                return Forbid();
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != null && int.Parse(currentUserId) == id)
            {
                return BadRequest("No puedes eliminar tu propia cuenta.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
