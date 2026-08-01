using Canchas.Api.WebApp.Data;
using Canchas.Api.WebApp.DTOS;
using Canchas.Api.WebApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Canchas.Api.WebApp.Services
{
    public interface IReservaService
    {
        Task<List<CanchaOfertaDto>> ConsultarDisponibilidadClubAsync(int clubId, DateTime fecha);
        Task<ReservaReadDto> CrearReservaPresencialAsync(CrearReservaPresencialDto dto, int createdByUserId);
        Task<List<ReservaReadDto>> ObtenerReservasClubAsync(int clubId, DateTime? fecha = null);
        Task CancelarReservaAsync(int reservaId, int userId, bool isSuperAdmin, int? userClubId);
    }

    public class ReservaService : IReservaService
    {
        private readonly AppDbContext _context;

        public ReservaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CanchaOfertaDto>> ConsultarDisponibilidadClubAsync(int clubId, DateTime fecha)
        {
            var fechaInicioDia = fecha.Date;
            var fechaFinDia = fecha.Date.AddDays(1);

            // 1. Obtener las canchas activas del club con sus fotos
            var canchas = await _context.Canchas
                .Include(c => c.Fotos)
                .Where(c => c.ClubId == clubId && c.Activa)
                .ToListAsync();

            var canchaIds = canchas.Select(c => c.Id).ToList();

            // 2. Cargar reservas activas del día
            var reservasDelDia = await _context.Reservas
                .Where(r => canchaIds.Contains(r.CanchaId) &&
                            r.Estado != EstadoReserva.Cancelada &&
                            r.FechaInicio < fechaFinDia &&
                            r.FechaFin > fechaInicioDia)
                .ToListAsync();

            // 3. Cargar bloqueos activos del día
            var bloqueosDelDia = await _context.CanchaBloqueos
                .Where(b => canchaIds.Contains(b.CanchaId) &&
                            b.FechaInicio < fechaFinDia &&
                            b.FechaFin > fechaInicioDia)
                .ToListAsync();

            var resultado = new List<CanchaOfertaDto>();

            foreach (var cancha in canchas)
            {
                var fotoPrincipal = cancha.Fotos.FirstOrDefault(f => f.EsPrincipal)?.Url
                                    ?? cancha.Fotos.OrderBy(f => f.Orden).FirstOrDefault()?.Url;

                var slots = GenerarSlotsParaCancha(cancha, fecha, reservasDelDia, bloqueosDelDia);

                resultado.Add(new CanchaOfertaDto
                {
                    CanchaId = cancha.Id,
                    NombreCancha = cancha.Nombre,
                    TipoCancha = cancha.TipoCancha,
                    PrecioHoraBase = cancha.PrecioHora,
                    FotoPrincipalUrl = fotoPrincipal,
                    HorariosDisponibles = slots
                });
            }

            return resultado;
        }

        public async Task<ReservaReadDto> CrearReservaPresencialAsync(CrearReservaPresencialDto dto, int createdByUserId)
        {
            var cancha = await _context.Canchas.FindAsync(dto.CanchaId);
            if (cancha == null || !cancha.Activa)
            {
                throw new InvalidOperationException("La cancha especificada no existe o se encuentra inactiva.");
            }

            // Validar traslape con otras reservas no canceladas
            var existeTraslapeReserva = await _context.Reservas.AnyAsync(r =>
                r.CanchaId == dto.CanchaId &&
                r.Estado != EstadoReserva.Cancelada &&
                r.FechaInicio < dto.FechaFin &&
                r.FechaFin > dto.FechaInicio);

            if (existeTraslapeReserva)
            {
                throw new InvalidOperationException("El horario seleccionado ya se encuentra reservado.");
            }

            // Validar traslape con bloqueos de la cancha
            var existeTraslapeBloqueo = await _context.CanchaBloqueos.AnyAsync(b =>
                b.CanchaId == dto.CanchaId &&
                b.FechaInicio < dto.FechaFin &&
                b.FechaFin > dto.FechaInicio);

            if (existeTraslapeBloqueo)
            {
                throw new InvalidOperationException("El horario seleccionado está bloqueado por mantenimiento o administración.");
            }

            // Crear entidad
            var reserva = new Reserva
            {
                CanchaId = dto.CanchaId,
                ClienteId = dto.ClienteId,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                MontoTotal = dto.MontoTotal,
                Estado = dto.Pagado ? EstadoReserva.Confirmada : EstadoReserva.Pendiente,
                MetodoPago = dto.Pagado ? dto.MetodoPago : null,
                MontoPagado = dto.Pagado ? dto.MontoTotal : null,
                FechaPagoReal = dto.Pagado ? DateTime.UtcNow : null,
                CreatedByUserId = createdByUserId
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return await MapearAReservaReadDto(reserva.Id);
        }

        public async Task<List<ReservaReadDto>> ObtenerReservasClubAsync(int clubId, DateTime? fecha = null)
        {
            var query = _context.Reservas
                .Include(r => r.Cancha)
                .Include(r => r.Cliente)
                .Include(r => r.CreatedByUser)
                .Where(r => r.Cancha.ClubId == clubId);

            if (fecha.HasValue)
            {
                var inicioDia = fecha.Value.Date;
                var finDia = inicioDia.AddDays(1);
                query = query.Where(r => r.FechaInicio >= inicioDia && r.FechaInicio < finDia);
            }

            return await query
                .OrderByDescending(r => r.FechaInicio)
                .Select(r => new ReservaReadDto
                {
                    Id = r.Id,
                    CanchaId = r.CanchaId,
                    NombreCancha = r.Cancha.Nombre,
                    ClienteId = r.ClienteId,
                    NombreCliente = r.Cliente != null ? r.Cliente.Nombre : "Cliente Presencial",
                    FechaInicio = r.FechaInicio,
                    FechaFin = r.FechaFin,
                    MontoTotal = r.MontoTotal,
                    Estado = r.Estado,
                    MetodoPago = r.MetodoPago,
                    MontoPagado = r.MontoPagado,
                    FechaPagoReal = r.FechaPagoReal,
                    CreatedByUserId = r.CreatedByUserId,
                    CreadoPorUsuario = r.CreatedByUser.Nombre
                })
                .ToListAsync();
        }

        public async Task CancelarReservaAsync(int reservaId, int userId, bool isSuperAdmin, int? userClubId)
        {
            var reserva = await _context.Reservas.Include(r => r.Cancha).FirstOrDefaultAsync(r => r.Id == reservaId);
            if (reserva == null)
            {
                throw new KeyNotFoundException("Reserva no encontrada.");
            }

            if (!isSuperAdmin && reserva.Cancha.ClubId != userClubId)
            {
                throw new UnauthorizedAccessException("No tienes permiso para cancelar esta reserva.");
            }

            reserva.Estado = EstadoReserva.Cancelada;
            await _context.SaveChangesAsync();
        }

        // Genera los bloques según los horarios configurados en la Cancha
        private static List<SlotDisponibilidadDto> GenerarSlotsParaCancha(
            Cancha cancha,
            DateTime fecha,
            List<Reserva> reservas,
            List<CanchaBloqueo> bloqueos)
        {
            var slots = new List<SlotDisponibilidadDto>();

            var horaInicio = cancha.HorarioInicio ?? new TimeSpan(8, 0, 0);  // Default 08:00
            var horaFin = cancha.HorarioFin ?? new TimeSpan(23, 0, 0);       // Default 23:00
            var duracionMinutos = cancha.DuracionMinimaMinutos > 0 ? cancha.DuracionMinimaMinutos : 60;

            var inicioSlot = fecha.Date.Add(horaInicio);
            var finJornada = fecha.Date.Add(horaFin);

            while (inicioSlot.AddMinutes(duracionMinutos) <= finJornada)
            {
                var finSlot = inicioSlot.AddMinutes(duracionMinutos);

                var ocupadoPorReserva = reservas.FirstOrDefault(r =>
                    r.CanchaId == cancha.Id &&
                    r.FechaInicio < finSlot &&
                    r.FechaFin > inicioSlot);

                var ocupadoPorBloqueo = bloqueos.FirstOrDefault(b =>
                    b.CanchaId == cancha.Id &&
                    b.FechaInicio < finSlot &&
                    b.FechaFin > inicioSlot);

                bool disponible = ocupadoPorReserva == null && ocupadoPorBloqueo == null;
                string? motivo = null;

                if (ocupadoPorReserva != null) motivo = "Reservada";
                else if (ocupadoPorBloqueo != null) motivo = string.IsNullOrWhiteSpace(ocupadoPorBloqueo.Motivo)
                    ? "Bloqueada por mantenimiento"
                    : ocupadoPorBloqueo.Motivo;

                slots.Add(new SlotDisponibilidadDto
                {
                    FechaInicio = inicioSlot,
                    FechaFin = finSlot,
                    Precio = cancha.PrecioHora,
                    Disponible = disponible,
                    MotivoOcupado = motivo
                });

                inicioSlot = finSlot; // Avanza el bloque
            }

            return slots;
        }

        private async Task<ReservaReadDto> MapearAReservaReadDto(int reservaId)
        {
            return await _context.Reservas
                .Include(r => r.Cancha)
                .Include(r => r.Cliente)
                .Include(r => r.CreatedByUser)
                .Where(r => r.Id == reservaId)
                .Select(r => new ReservaReadDto
                {
                    Id = r.Id,
                    CanchaId = r.CanchaId,
                    NombreCancha = r.Cancha.Nombre,
                    ClienteId = r.ClienteId,
                    NombreCliente = r.Cliente != null ? r.Cliente.Nombre : "Cliente Presencial",
                    FechaInicio = r.FechaInicio,
                    FechaFin = r.FechaFin,
                    MontoTotal = r.MontoTotal,
                    Estado = r.Estado,
                    MetodoPago = r.MetodoPago,
                    MontoPagado = r.MontoPagado,
                    FechaPagoReal = r.FechaPagoReal,
                    CreatedByUserId = r.CreatedByUserId,
                    CreadoPorUsuario = r.CreatedByUser.Nombre
                })
                .FirstAsync();
        }
    }
}
