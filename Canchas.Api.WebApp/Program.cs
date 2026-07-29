using Canchas.Api.WebApp;
using Canchas.Api.WebApp.Data; // Asegúrate de tener esta referencia para el seeder
using Canchas.Api.WebApp.Models;
using Canchas.Api.WebApp.Providers;
using Canchas.Api.WebApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3039"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ConexionDataBase"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ConexionDataBase"))
    )
    .LogTo(Console.WriteLine, LogLevel.Information));

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Canchas.WebAPI", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Escribe: 'Bearer' seguido de un espacio y tu token.\n\nEjemplo: Bearer eyJhbGci..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<AvailabilityService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// --- INICIALIZACIÓN DE DATOS (SEEDS) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        // 1. Cargar Regiones y Comunas de Chile
        // Se llama de forma asíncrona pero esperamos el resultado (.Wait o await)
        LocationSeeder.SeedLocationData(context).Wait();
        Console.WriteLine("--> Datos de localización verificados/cargados.");

        // 2. Si no hay usuarios, creamos el SuperAdmin inicial
        if (!context.Users.Any())
        {
            var superAdmin = new User
            {
                Email = "admin@canchas.cl",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Canchas2026!"),
                Rol = RolUsuario.SuperAdmin,
                ClubId = null
            };

            context.Users.Add(superAdmin);
            context.SaveChanges();

            Console.WriteLine("--> SuperAdmin creado con ID 1 y Hash verificado.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Error en el Seeder: {ex.Message}");
    }
}

app.Run();
