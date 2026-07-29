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
            Pendiente, Confirmada, Pagada, Cancelada
        }

        public enum RolUsuario
        {
            SuperAdmin, ClubAdmin, AgendaCreator, CourtManager, Cliente
        }

        public enum MetodoPago
        {
            Efectivo, Transferencia, Transbank, MercadoPago, Otro
        }

    
}
