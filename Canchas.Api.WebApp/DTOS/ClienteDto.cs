namespace Canchas.Api.WebApp.DTOS
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string? Email { get; set; }
        public string? Telefono { get; set; }
    }
}
