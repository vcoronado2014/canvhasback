using Canchas.Api.WebApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Canchas.Api.WebApp
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets
        public DbSet<Club> Clubs { get; set; }
        public DbSet<Cancha> Canchas { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Disponibilidad> Disponibilidades { get; set; }
        public DbSet<SuscripcionClub> SuscripcionesClubs { get; set; }
        public DbSet<PagoLog> PagoLogs { get; set; }
        public DbSet<RegionChile> Regiones { get; set; }
        public DbSet<ComunaChile> ComunasChile { get; set; }
        //nuevas entidades
        public DbSet<CanchaFoto> CanchaFotos { get; set; }
        public DbSet<CanchaHorarioTarifa> CanchaHorarioTarifas { get; set; }
        public DbSet<CanchaBloqueo> CanchaBloqueos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // JSON Options compartidas
            var jsonOptions = new JsonSerializerOptions();

            // ValueComparer reutilizable para List<string>
            static ValueComparer<List<string>> ListStringComparer() => new(
                (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2 ?? new List<string>())),
                c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c == null ? new List<string>() : c.ToList());

            // === CLUB ===
            modelBuilder.Entity<Club>(entity =>
            {
                // JSON Properties con ValueComparer corregido
                entity.Property(e => e.MetodosPagoHabilitados)
                    .HasColumnType("JSON")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions),
                        ListStringComparer());

                entity.Property(e => e.ConfigPagos)
                    .HasColumnType("JSON")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, jsonOptions));

                entity.Property(e => e.AmenitiesJson)
                    .HasColumnType("JSON")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions),
                        ListStringComparer());

                // Enum → string
                entity.Property(e => e.EstadoSuscripcion).HasConversion<string>();
            });

            // === PAGO LOG ===
            modelBuilder.Entity<PagoLog>(entity =>
            {
                entity.Property(e => e.ResponseData)
                    .HasColumnType("JSON")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<object>(v, jsonOptions));
            });

            // === ENUMS → STRING (MySQL VARCHAR) ===
            modelBuilder.Entity<Reserva>().Property(e => e.Estado).HasConversion<string>();
            modelBuilder.Entity<Reserva>().Property(e => e.MetodoPago).HasConversion<string>();
            modelBuilder.Entity<User>().Property(e => e.Rol).HasConversion<string>();
            modelBuilder.Entity<Cancha>().Property(e => e.TipoCancha).HasConversion<string>();
            modelBuilder.Entity<SuscripcionClub>().Property(e => e.Estado).HasConversion<string>();
            modelBuilder.Entity<SuscripcionClub>().Property(e => e.MetodoPagoSuscripcion).HasConversion<string>();

            // === ÍNDICES (Performance + Unique) ===
            modelBuilder.Entity<Club>()
                .HasIndex(e => e.Subdominio)
                .IsUnique();

            modelBuilder.Entity<Club>()
                .HasIndex(e => new { e.Latitud, e.Longitud });  // Geoloc queries

            modelBuilder.Entity<User>()
                .HasIndex(e => e.Email)
                .IsUnique();

            modelBuilder.Entity<PagoLog>()
                .HasIndex(e => new { e.ReservaId, e.CreatedAt });

            modelBuilder.Entity<PagoLog>()
                .HasIndex(e => e.Status);

            modelBuilder.Entity<Reserva>()
                .HasIndex(e => e.FechaInicio);  // Calendario queries

            // === RELACIONES FK ===
            // Club → OwnerUser
            modelBuilder.Entity<Club>()
                .HasOne(e => e.OwnerUser)
                .WithMany()
                .HasForeignKey(e => e.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);  // No borra user si elimina club

            // Reserva → CreatedByUser + Cancha + Cliente
            modelBuilder.Entity<Reserva>()
                .HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserva>()
                .HasOne(e => e.Cancha)
                .WithMany(e => e.Reservas)
                .HasForeignKey(e => e.CanchaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserva>()
                .HasOne(e => e.Cliente)
                .WithMany(e => e.Reservas)
                .HasForeignKey(e => e.ClienteId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // PagoLog → Reserva (cascade delete)
            modelBuilder.Entity<PagoLog>()
                .HasOne(e => e.Reserva)
                .WithMany(e => e.PagoLogs)
                .HasForeignKey(e => e.ReservaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Otras relaciones estándar (Cancha→Club, etc.) EF infiere

            modelBuilder.Entity<ComunaChile>()
                .HasOne(c => c.Region)
                .WithMany(r => r.Comunas)
                .HasForeignKey(c => c.RegionCodigo)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}