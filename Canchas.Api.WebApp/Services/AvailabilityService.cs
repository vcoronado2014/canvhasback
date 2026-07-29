using Canchas.Api.WebApp.DTOS;
using Canchas.Api.WebApp.Models;
using Microsoft.EntityFrameworkCore;


namespace Canchas.Api.WebApp.Services
{
    public class AvailabilityService
    {
        private readonly AppDbContext _context;

        public AvailabilityService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DisponibilidadDiaDto?> ObtenerBloquesDisponiblesAsync(int canchaId, DateTime fecha)
        {
            // 1. Obtener la cancha con sus tarifas
            var cancha = await _context.Canchas
                .Include(c => c.HorariosTarifas)
                .FirstOrDefaultAsync(c => c.Id == canchaId && c.Activa);

            if (cancha == null) return null;

            var inicioDia = fecha.Date;
            var finDia = inicioDia.AddDays(1).AddTicks(-1);

            // 2. Traer Bloqueos y Reservas de esa fecha específica
            var bloqueos = await _context.CanchaBloqueos
                .Where(b => b.CanchaId == canchaId && b.FechaInicio < finDia && b.FechaFin > inicioDia)
                .ToListAsync();

            var reservas = await _context.Reservas
                .Where(r => r.CanchaId == canchaId
                            && r.FechaInicio < finDia
                            && r.FechaFin > inicioDia
                            && r.Estado != EstadoReserva.Cancelada) // Excluimos canceladas
                .ToListAsync();

            // 3. Filtrar las tarifas configuradas para el día de la semana correspondiente
            DayOfWeek diaSemana = fecha.DayOfWeek;
            var tarifasDelDia = cancha.HorariosTarifas
                .Where(t => t.DiaSemana == diaSemana)
                .OrderBy(t => t.HoraInicio)
                .ToList();

            var resultado = new DisponibilidadDiaDto
            {
                CanchaId = canchaId,
                Fecha = inicioDia
            };

            // 4. Generar la grilla de bloques basándonos en los rangos de tarifas
            int intervalo = cancha.IntervaloMinutos > 0 ? cancha.IntervaloMinutos : 60;

            foreach (var tarifa in tarifasDelDia)
            {
                var horaActual = tarifa.HoraInicio;

                // Generamos los slots según la duración configurada en la cancha
                while (horaActual + TimeSpan.FromMinutes(intervalo) <= tarifa.HoraFin)
                {
                    var slotInicio = horaActual;
                    var slotFin = horaActual.Add(TimeSpan.FromMinutes(intervalo));

                    // Convertimos los slots a DateTime completo para comparar fácilmente con Reservas/Bloqueos
                    var dtSlotInicio = inicioDia.Add(slotInicio);
                    var dtSlotFin = inicioDia.Add(slotFin);

                    // Verificamos si se solapa con alguna Reserva
                    bool estaReservado = reservas.Any(r => r.FechaInicio < dtSlotFin && r.FechaFin > dtSlotInicio);

                    // Verificamos si se solapa con algún Bloqueo de mantenimiento
                    var bloqueo = bloqueos.FirstOrDefault(b => b.FechaInicio < dtSlotFin && b.FechaFin > dtSlotInicio);

                    bool estaDisponible = !estaReservado && bloqueo == null;
                    string? motivo = null;

                    if (estaReservado) motivo = "Reservado";
                    else if (bloqueo != null) motivo = bloqueo.Motivo ?? "Mantenimiento / No disponible";

                    resultado.Bloques.Add(new BloqueHorarioDto
                    {
                        HoraInicio = slotInicio,
                        HoraFin = slotFin,
                        Precio = tarifa.PrecioPorBloque,
                        Disponible = estaDisponible,
                        MotivoIndisponibilidad = motivo
                    });

                    // Avanzamos al siguiente slot
                    horaActual = slotFin;
                }
            }

            return resultado;
        }
    }
}
