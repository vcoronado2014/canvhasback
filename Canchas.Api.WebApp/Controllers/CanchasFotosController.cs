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
    public class CanchasFotosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CanchasFotosController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [Authorize(Roles = "SuperAdmin,ClubAdmin,CourtManager")]
        [HttpPost("cancha/{canchaId}/subir")]
        public async Task<IActionResult> SubirFoto(int canchaId, [FromForm] SubirFotoCanchaRequest request)
        {
            if (request.Archivo == null || request.Archivo.Length == 0)
                return BadRequest("Debe seleccionar un archivo de imagen válido.");

            // 1. Validar existencia de la cancha y traer sus fotos actuales
            var cancha = await _context.Canchas
                .Include(c => c.Fotos)
                .FirstOrDefaultAsync(c => c.Id == canchaId);

            if (cancha == null)
                return NotFound("La cancha no fue encontrada.");

            try
            {
                // 2. Definir carpeta de destino (wwwroot/uploads/canchas)
                string folderPath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "canchas");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // 3. Generar un nombre único para evitar colisiones
                string extension = Path.GetExtension(request.Archivo.FileName);
                string fileName = $"{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Archivo.CopyToAsync(stream);
                }

                // 4. Crear el registro en la BD
                string relativeUrl = $"/uploads/canchas/{fileName}";

                var nuevaFoto = new CanchaFoto
                {
                    CanchaId = canchaId,
                    Url = relativeUrl,
                    Orden = request.Orden > 0 ? request.Orden : cancha.Fotos.Count + 1,
                    EsPrincipal = !cancha.Fotos.Any() || request.EsPrincipal
                };

                _context.CanchaFotos.Add(nuevaFoto);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    nuevaFoto.Id,
                    nuevaFoto.CanchaId,
                    nuevaFoto.Url,
                    nuevaFoto.EsPrincipal,
                    nuevaFoto.Orden
                });
            }
            catch
            {
                return StatusCode(500, "Error interno al procesar y guardar la imagen.");
            }
        }

        [Authorize(Roles = "SuperAdmin,ClubAdmin,CourtManager,AgendaCreator")]
        [HttpGet("cancha/{canchaId}")]
        public async Task<IActionResult> ObtenerFotosCancha(int canchaId)
        {
            var fotos = await _context.CanchaFotos
                .Where(f => f.CanchaId == canchaId)
                .OrderBy(f => f.Orden)
                .Select(f => new
                {
                    f.Id,
                    f.Url,
                    f.EsPrincipal,
                    f.Orden
                })
                .ToListAsync();

            return Ok(fotos);
        }

        [Authorize(Roles = "SuperAdmin,ClubAdmin,CourtManager")]
        [HttpDelete("{fotoId}")]
        public async Task<IActionResult> EliminarFoto(int fotoId)
        {
            var foto = await _context.CanchaFotos.FindAsync(fotoId);
            if (foto == null)
                return NotFound("La imagen no existe.");

            // 1. Eliminar archivo físico del servidor
            if (!string.IsNullOrEmpty(foto.Url))
            {
                string physicalPath = Path.Combine(
                    _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                    foto.Url.TrimStart('/')
                );

                if (System.IO.File.Exists(physicalPath))
                    System.IO.File.Delete(physicalPath);
            }

            // 2. Eliminar registro de la BD
            _context.CanchaFotos.Remove(foto);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Imagen eliminada con éxito." });
        }
    }
}
