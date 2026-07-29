using Canchas.Api.WebApp.DTOS;
using Canchas.Api.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Canchas.Api.WebApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context) => _context = context;

        [HttpPost("crear-staff")]
        public async Task<IActionResult> CrearStaff([FromBody] CreateStaffRequest request)
        {
            // 1. Obtener datos del usuario que hace la petición (desde el JWT)
            var userRol = User.FindFirstValue(ClaimTypes.Role);
            var userClubIdStr = User.FindFirstValue("clubId");

            // 2. Validaciones de Seguridad
            if (userRol == "ClubAdmin")
            {
                // El ClubAdmin está obligado a crear usuarios solo para su club
                if (string.IsNullOrEmpty(userClubIdStr)) return Forbid();
                request.ClubId = int.Parse(userClubIdStr);

                // Un ClubAdmin NO puede crear un SuperAdmin
                if (request.Rol == RolUsuario.SuperAdmin)
                    return BadRequest("No tienes permisos para crear este rol.");
            }
            else if (userRol != "SuperAdmin")
            {
                return Forbid(); // AgendaCreator o CourtManager no pueden crear usuarios
            }

            // 3. Crear el usuario de staff (AgendaCreator, CourtManager, etc.)
            var nuevoUsuario = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Rol = request.Rol,
                ClubId = request.ClubId // Si es SuperAdmin, el ClubId viene en el body
            };

            _context.Users.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Usuario {request.Rol} creado correctamente." });
        }
    }
}
