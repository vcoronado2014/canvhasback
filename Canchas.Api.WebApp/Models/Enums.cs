namespace Canchas.Api.WebApp.Models
{
    
        public enum TipoCancha
        {
            PadelDoble, PadelSingle, Futbolito7, Futbolito8, Tenis, Multi, Otro
        }

        public enum EstadoSuscripcionClub
        {
            Activo, PendientePago, Suspendido, Cancelado
        }

        public enum EstadoReserva
        {
            Pendiente = 0,   // Reservada pero aún no se paga en el club
            Confirmada = 1,  // Confirmada / Pagada presencialmente
            Completada = 2,  // Ya se jugó la hora
            Cancelada = 3    // Cancelada por el usuario o administración
        }

        public enum RolUsuario
        {
            SuperAdmin, ClubAdmin, AgendaCreator, CourtManager, Cliente
        }

        public enum MetodoPago
        {
            Efectivo = 0,
            Debito = 1,
            Credito = 2,
            Transferencia = 3,
            PresencialServicio = 4 // Pago directo en el club al llegar
        }


}
