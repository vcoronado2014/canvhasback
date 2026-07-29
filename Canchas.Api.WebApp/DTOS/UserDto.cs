namespace Canchas.Api.WebApp.DTOS
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string Rol { get; set; } = "";
        public int? ClubId { get; set; }
        public string Nombre { get; set; } = "";
        public string Telefono { get; set; } = "";
    }
}
