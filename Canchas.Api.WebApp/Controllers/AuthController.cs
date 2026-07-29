using Canchas.Api.WebApp;
using Canchas.Api.WebApp.DTOS;
using Canchas.Api.WebApp.Models;
using Canchas.Api.WebApp.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IJwtProvider _jwtProvider;

    public AuthController(AppDbContext context, IJwtProvider jwtProvider)
    {
        _context = context;
        _jwtProvider = jwtProvider;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 1. Intentamos buscar primero en la tabla de Usuarios (Staff/Admin)
        var user = await _context.Users
            .Include(u => u.Club)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user != null && VerifyPassword(request.Password, user.PasswordHash))
        {
            var authDto = new AuthUserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                Nombre = user.Nombre, // Puedes agregar 'Nombre' a la clase User luego
                Rol = user.Rol.ToString(),
                ClubId = user.ClubId,
                Telefono = user.Telefono,
            };

            return Ok(new { 
                Token = _jwtProvider.Generate(authDto), 
                Tipo = "Staff",
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Rol = user.Rol.ToString(),
                    ClubId = user.ClubId,
                    Nombre = user.Nombre,
                    Telefono = user.Telefono
                },
                Club = user.Club == null ? null : new ClubDto
                {
                    Id = user.Club.Id,
                    Nombre = user.Club.Nombre,
                    Direccion = user.Club.Direccion,
                    Telefono = user.Club.Telefono,
                    RegionCodigo = user.Club.RegionCodigo,
                    RegionNombre = user.Club.RegionNombre,
                    ComunaCodigo = user.Club.ComunaCodigo,
                    ComunaNombre = user.Club.ComunaNombre,
                    Latitud = user.Club.Latitud,
                    Longitud = user.Club.Longitud,
                    FotoPrincipalUrl = user.Club.FotoPrincipalUrl,
                    Descripcion = user.Club.Descripcion,
                    Subdominio = user.Club.Subdominio,
                    EstadoSuscripcion = user.Club.EstadoSuscripcion,
                    FechaProxVencimiento = user.Club.FechaProxVencimiento
                }

            });
        }

        // 2. Si no es Staff, buscamos en la tabla de Clientes (Jugadores)
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Email == request.Email);

        if (cliente != null && VerifyPassword(request.Password, cliente.PasswordHash))
        {
            var authDto = new AuthUserDto
            {
                Id = cliente.Id.ToString(),
                Email = cliente.Email ?? "",
                Nombre = cliente.Nombre,
                Rol = "Cliente"
            };

            return Ok(new { 
                Token = _jwtProvider.Generate(authDto), 
                Tipo = "Cliente",
                Cliente = new ClienteDto
                {
                    Id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Email = cliente.Email,
                    Telefono = cliente.Telefono
                }
            });
        }

        return Unauthorized("Email o contraseña incorrectos");
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
    [FromBody] RegistroClienteRequest request)
    {
        if (await _context.Clientes.AnyAsync(x => x.Email == request.Email))
        {
            return BadRequest("Ya existe un cliente con ese email.");
        }

        var cliente = new Cliente
        {
            Nombre = request.Nombre,
            Email = request.Email,
            Telefono = request.Telefono,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _context.Clientes.Add(cliente);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            cliente.Id,
            cliente.Nombre,
            cliente.Email
        });
    }

    // Método auxiliar para verificar el hash (ajústalo según tu método de cifrado)
    private bool VerifyPassword(string password, string? hash)
    {
        if (string.IsNullOrEmpty(hash)) return false;
        // Aquí deberías usar BCrypt o IdentityPasswordHasher. 
        // Ejemplo simple si guardaste el hash directamente (no recomendado para producción):
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
