namespace Canchas.Api.WebApp.DTOS
{
    public class AuthUserDto
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Rol { get; set; } = ""; // "Cliente", "Admin", "Staff", etc.
        public int? ClubId { get; set; }
    }
}
