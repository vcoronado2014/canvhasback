using Canchas.Api.WebApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Canchas.Api.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisponibilidadController : ControllerBase
    {
        private readonly AvailabilityService _availabilityService;

        public DisponibilidadController(AvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDisponibilidad(int canchaId, [FromQuery] DateTime fecha)
        {
            var respuesta = await _availabilityService.ObtenerBloquesDisponiblesAsync(canchaId, fecha);

            if (respuesta == null)
                return NotFound("La cancha no existe o se encuentra desactivada.");

            return Ok(respuesta);
        }
    }
}
