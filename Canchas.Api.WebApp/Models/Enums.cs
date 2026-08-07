namespace Canchas.Api.WebApp.Models
{

    public enum TipoCancha
    {
        PadelDoble = 0,
        PadelSingle = 1,
        Futbolito7 = 2,
        Futbolito8 = 3,
        Tenis = 4,
        Multicancha = 5,
        Otro = 6,

        Futbol11 = 7,
        FutbolitoTechado = 8,
        Raquetbol = 9,
        Squash = 10,
        Hockey = 11,
        MulticanchaTechada = 12,
        Rugby = 13,
        EKarting = 14,
        TenisMesa = 15,
        Futbol6 = 16,
        Voleibol = 17,
        Handball = 18,
        Futbol9 = 19
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
