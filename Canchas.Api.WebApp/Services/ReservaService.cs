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
        Task<List<CanchaOfertaDto>> ConsultarDisponibilidadClubAsync(int clubId, DateTime fechaInicio, DateTime? fechaFin = null);
        Task<List<ReservaReadDto>> ObtenerReservasClubAsync(int clubId, DateTime? fechaInicio = null, DateTime? fechaFin = null);
        Task<List<ReservaReadDto>> ObtenerReservasPorClienteAsync(int clienteUserId); // Faltaba aquí
        Task<ReservaReadDto> CrearReservaClienteAsync(CrearReservaClienteDto dto, int userId); // Faltaba aquí
        Task<ReservaReadDto> CrearReservaPresencialAsync(CrearReservaPresencialDto dto, int userId);
        Task CancelarReservaAsync(int reservaId, int userId, bool isSuperAdmin, int? userClubId);
    }

    public class ReservaService : IReservaService
    {
        private readonly AppDbContext _context;

        public ReservaService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Genera la oferta/grilla de disponibilidad de todas las canchas del club,
        /// cruzando los bloques de disponibilidad con las reservas activas.
        /// </summary>
        public async Task<List<CanchaOfertaDto>> ConsultarDisponibilidadClubAsync(
            int clubId,
            DateTime fechaInicio,
            DateTime? fechaFin = null)
        {
            var club = await _context.Clubs
                .Include(c => c.Canchas)
                    .ThenInclude(ca => ca.Fotos)
                .FirstOrDefaultAsync(c => c.Id == clubId);

            if (club == null) return new List<CanchaOfertaDto>();

            var inicioFiltro = fechaInicio.Date;
            var finFiltro = fechaFin.HasValue ? fechaFin.Value.Date : inicioFiltro;
            var limiteExclusivo = finFiltro.AddDays(1);

            var fotoClubFallback = club.FotoPrincipalUrl
                ?? club.Canchas.SelectMany(c => c.Fotos).FirstOrDefault()?.Url;

            var disponibilidades = await _context.Disponibilidades
                .Where(d => d.Cancha.ClubId == clubId
                         && d.Fecha >= inicioFiltro
                         && d.Fecha < limiteExclusivo)
                .ToListAsync();

            // Traer reservas activas ampliando el margen para evitar cortes por UTC
            var reservasActivas = await _context.Reservas
                .Where(r => r.Cancha.ClubId == clubId
                         && r.FechaInicio < limiteExclusivo
                         && r.FechaFin > inicioFiltro
                         && r.Estado != EstadoReserva.Cancelada)
                .ToListAsync();

            var resultado = new List<CanchaOfertaDto>();

            foreach (var cancha in club.Canchas)
            {
                var horarios = new List<SlotDisponibilidadDto>();
                var dispoCancha = disponibilidades.Where(d => d.CanchaId == cancha.Id);

                int pasoMinutos = cancha.DuracionMinimaMinutos > 0 ? cancha.DuracionMinimaMinutos : 60;

                foreach (var disp in dispoCancha)
                {
                    var ventanaInicio = disp.Fecha.Date.Add(disp.HoraInicio);
                    var ventanaFin = disp.Fecha.Date.Add(disp.HoraFin);

                    // Generar sub-bloques exactos dentro del horario operativo
                    var slotActual = ventanaInicio;
                    while (slotActual.AddMinutes(pasoMinutos) <= ventanaFin)
                    {
                        var slotFin = slotActual.AddMinutes(pasoMinutos);

                        // Comprobar si este sub-bloque específico colisiona con alguna reserva activa
                        var reservaExistente = reservasActivas.FirstOrDefault(r =>
                            r.CanchaId == cancha.Id &&
                            r.FechaInicio < slotFin &&
                            r.FechaFin > slotActual);

                        horarios.Add(new SlotDisponibilidadDto
                        {
                            FechaInicio = slotActual,
                            FechaFin = slotFin,
                            Precio = cancha.PrecioHora,
                            Disponible = reservaExistente == null,
                            MotivoOcupado = reservaExistente != null ? "Reservado" : disp.Motivo
                        });

                        slotActual = slotFin;
                    }
                }

                resultado.Add(new CanchaOfertaDto
                {
                    ClubId = club.Id,
                    NombreClub = club.Nombre,
                    DireccionClub = club.Direccion,
                    ComunaNombre = club.ComunaNombre,
                    RegionNombre = club.RegionNombre,
                    FotoClubUrl = fotoClubFallback,

                    CanchaId = cancha.Id,
                    NombreCancha = cancha.Nombre,
                    TipoCancha = cancha.TipoCancha,
                    PrecioHoraBase = cancha.PrecioHora,
                    FotoPrincipalUrl = cancha.Fotos.FirstOrDefault()?.Url,
                    HorariosDisponibles = horarios.OrderBy(h => h.FechaInicio).ToList(),
                    DuracionMinimaMinutos = cancha.DuracionMinimaMinutos
                });
            }

            return resultado;
        }

        //public async Task<List<CanchaOfertaDto>> ConsultarDisponibilidadClubAsync(
        //    int clubId,
        //    DateTime fechaInicio,
        //    DateTime? fechaFin = null)
        //{
        //    var club = await _context.Clubs
        //        .Include(c => c.Canchas)
        //            .ThenInclude(ca => ca.Fotos)
        //        .FirstOrDefaultAsync(c => c.Id == clubId);

        //    if (club == null) return new List<CanchaOfertaDto>();

        //    // 1. Resolver fecha de fin si no se especifica (por defecto, el mismo día de fechaInicio)
        //    var inicioFiltro = fechaInicio.Date;
        //    var finFiltro = fechaFin.HasValue ? fechaFin.Value.Date : inicioFiltro;

        //    // Agregamos 1 día para incluir todas las horas del día límite
        //    var limiteExclusivo = finFiltro.AddDays(1);

        //    // 2. Foto del club o fallback a la foto de alguna cancha
        //    var fotoClubFallback = club.FotoPrincipalUrl
        //        ?? club.Canchas.SelectMany(c => c.Fotos).FirstOrDefault()?.Url;

        //    // 3. Consultar disponibilidades dentro del rango
        //    var disponibilidades = await _context.Disponibilidades
        //        .Where(d => d.Cancha.ClubId == clubId
        //                 && d.Fecha >= inicioFiltro
        //                 && d.Fecha < limiteExclusivo)
        //        .ToListAsync();

        //    // 4. Consultar reservas dentro del rango
        //    var reservasActivas = await _context.Reservas
        //        .Where(r => r.Cancha.ClubId == clubId
        //                 && r.FechaInicio >= inicioFiltro
        //                 && r.FechaFin < limiteExclusivo
        //                 && r.Estado != EstadoReserva.Cancelada)
        //        .ToListAsync();

        //    var resultado = new List<CanchaOfertaDto>();

        //    foreach (var cancha in club.Canchas)
        //    {
        //        var horarios = new List<SlotDisponibilidadDto>();
        //        var dispoCancha = disponibilidades.Where(d => d.CanchaId == cancha.Id);

        //        foreach (var disp in dispoCancha)
        //        {
        //            // disp.Fecha es DateTime (no nullable), así que usamos directamente .Date
        //            var fechaInicioSlot = disp.Fecha.Date.Add(disp.HoraInicio);
        //            var fechaFinSlot = disp.Fecha.Date.Add(disp.HoraFin);

        //            var reservaExistente = reservasActivas.FirstOrDefault(r =>
        //                r.CanchaId == cancha.Id &&
        //                r.FechaInicio < fechaFinSlot &&
        //                r.FechaFin > fechaInicioSlot);

        //            horarios.Add(new SlotDisponibilidadDto
        //            {
        //                FechaInicio = fechaInicioSlot,
        //                FechaFin = fechaFinSlot,
        //                Precio = cancha.PrecioHora,
        //                Disponible = reservaExistente == null,
        //                MotivoOcupado = reservaExistente != null ? "Reservado" : disp.Motivo
        //            });
        //        }

        //        resultado.Add(new CanchaOfertaDto
        //        {
        //            ClubId = club.Id,
        //            NombreClub = club.Nombre,
        //            DireccionClub = club.Direccion,
        //            ComunaNombre = club.ComunaNombre,
        //            RegionNombre = club.RegionNombre,
        //            FotoClubUrl = fotoClubFallback,

        //            CanchaId = cancha.Id,
        //            NombreCancha = cancha.Nombre,
        //            TipoCancha = cancha.TipoCancha,
        //            PrecioHoraBase = cancha.PrecioHora,
        //            FotoPrincipalUrl = cancha.Fotos.FirstOrDefault()?.Url,
        //            HorariosDisponibles = horarios.OrderBy(h => h.FechaInicio).ToList(),
        //            DuracionMinimaMinutos = cancha.DuracionMinimaMinutos
        //        });
        //    }

        //    return resultado;
        //}


        public async Task<List<ReservaReadDto>> ObtenerReservasClubAsync(int clubId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = _context.Reservas
                .Include(r => r.Cancha)
                .Include(r => r.Cliente)
                .Where(r => r.Cancha.ClubId == clubId);

            if (fechaInicio.HasValue)
                query = query.Where(r => r.FechaInicio >= fechaInicio.Value.Date);

            if (fechaFin.HasValue)
                query = query.Where(r => r.FechaInicio < fechaFin.Value.Date.AddDays(1));

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
                    CreatedByUserId = r.CreatedByUserId
                })
                .ToListAsync();
        }

        public async Task<List<ReservaReadDto>> ObtenerReservasPorClienteAsync(int clienteUserId)
        {
            return await _context.Reservas
                .Include(r => r.Cancha)
                .Where(r => r.ClienteId == clienteUserId)
                .OrderByDescending(r => r.FechaInicio)
                .Select(r => new ReservaReadDto
                {
                    Id = r.Id,
                    CanchaId = r.CanchaId,
                    NombreCancha = r.Cancha.Nombre,
                    FechaInicio = r.FechaInicio,
                    FechaFin = r.FechaFin,
                    MontoTotal = r.MontoTotal,
                    Estado = r.Estado,
                    MetodoPago = r.MetodoPago
                })
                .ToListAsync();
        }

        public async Task<ReservaReadDto> CrearReservaClienteAsync(CrearReservaClienteDto dto, int userId)
        {
            // Validar traslape
            bool ocupado = await _context.Reservas.AnyAsync(r =>
                r.CanchaId == dto.CanchaId &&
                r.Estado != EstadoReserva.Cancelada &&
                r.FechaInicio < dto.FechaFin &&
                r.FechaFin > dto.FechaInicio);

            if (ocupado)
                throw new InvalidOperationException("El horario seleccionado ya no se encuentra disponible.");

            var reserva = new Reserva
            {
                CanchaId = dto.CanchaId,
                ClienteId = userId,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                MontoTotal = dto.MontoTotal,
                Estado = EstadoReserva.Confirmada,
                MetodoPago = dto.MetodoPago,
                CreatedByUserId = userId,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return await MapearReservaReadDto(reserva.Id);
        }

        public async Task<ReservaReadDto> CrearReservaPresencialAsync(CrearReservaPresencialDto dto, int userId)
        {
            bool ocupado = await _context.Reservas.AnyAsync(r =>
                r.CanchaId == dto.CanchaId &&
                r.Estado != EstadoReserva.Cancelada &&
                r.FechaInicio < dto.FechaFin &&
                r.FechaFin > dto.FechaInicio);

            if (ocupado)
                throw new InvalidOperationException("El bloque seleccionado ya está ocupado por otra reserva.");

            var reserva = new Reserva
            {
                CanchaId = dto.CanchaId,
                ClienteId = dto.ClienteId,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                MontoTotal = dto.MontoTotal,
                Estado = dto.Pagado ? EstadoReserva.Confirmada : EstadoReserva.Pendiente,
                MetodoPago = dto.MetodoPago,
                CreatedByUserId = userId,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return await MapearReservaReadDto(reserva.Id);
        }

        public async Task CancelarReservaAsync(int reservaId, int userId, bool isSuperAdmin, int? clubId)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Cancha)
                .FirstOrDefaultAsync(r => r.Id == reservaId);

            if (reserva == null)
                throw new KeyNotFoundException();

            // Permisos: SuperAdmin, Staff del mismo club o el propio Cliente dueño de la reserva
            bool esStaffClub = clubId.HasValue && clubId.Value == reserva.Cancha.ClubId;
            bool esClienteDuenio = reserva.ClienteId.HasValue && reserva.ClienteId.Value == userId;

            if (!isSuperAdmin && !esStaffClub && !esClienteDuenio)
                throw new UnauthorizedAccessException();

            reserva.Estado = EstadoReserva.Cancelada;
            await _context.SaveChangesAsync();
        }

        private async Task<ReservaReadDto> MapearReservaReadDto(int reservaId)
        {
            var r = await _context.Reservas
                .Include(x => x.Cancha)
                .Include(x => x.Cliente)
                .FirstAsync(x => x.Id == reservaId);

            return new ReservaReadDto
            {
                Id = r.Id,
                CanchaId = r.CanchaId,
                NombreCancha = r.Cancha.Nombre,
                ClienteId = r.ClienteId,
                NombreCliente = r.Cliente?.Nombre ?? "Cliente Presencial",
                FechaInicio = r.FechaInicio,
                FechaFin = r.FechaFin,
                MontoTotal = r.MontoTotal,
                Estado = r.Estado,
                MetodoPago = r.MetodoPago,
                CreatedByUserId = r.CreatedByUserId,
                CreadoPorUsuario = r.Cliente?.Nombre ?? "Sistema/Staff"
            };
        }
    }
}
