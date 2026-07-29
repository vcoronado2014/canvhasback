using Canchas.Api.WebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace Canchas.Api.WebApp.Data
{
    public static class LocationSeeder
    {
        public static async Task SeedLocationData(AppDbContext context)
        {
            if (await context.Regiones.AnyAsync()) return;

            // 1. Insertar Regiones
            var regiones = new List<RegionChile>
            {
                new() { Codigo = "15", Nombre = "Arica y Parinacota" },
                new() { Codigo = "01", Nombre = "Tarapacá" },
                new() { Codigo = "02", Nombre = "Antofagasta" },
                new() { Codigo = "03", Nombre = "Atacama" },
                new() { Codigo = "04", Nombre = "Coquimbo" },
                new() { Codigo = "05", Nombre = "Valparaíso" },
                new() { Codigo = "RM", Nombre = "Metropolitana de Santiago" },
                new() { Codigo = "06", Nombre = "O'Higgins" },
                new() { Codigo = "07", Nombre = "Maule" },
                new() { Codigo = "16", Nombre = "Ñuble" },
                new() { Codigo = "08", Nombre = "Biobío" },
                new() { Codigo = "09", Nombre = "La Araucanía" },
                new() { Codigo = "14", Nombre = "Los Ríos" },
                new() { Codigo = "10", Nombre = "Los Lagos" },
                new() { Codigo = "11", Nombre = "Aysén" },
                new() { Codigo = "12", Nombre = "Magallanes" }
            };

            await context.Regiones.AddRangeAsync(regiones);
            await context.SaveChangesAsync();

            // 2. Insertar las 346 Comunas desde nuestra clase auxiliar
            var todasLasComunas = LocationData.GetAllComunas();
            await context.ComunasChile.AddRangeAsync(todasLasComunas);
            await context.SaveChangesAsync();
        }
    }
}