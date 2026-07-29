namespace Canchas.Api.WebApp.DTOS
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = "";
        public string Tipo { get; set; } = "";

        public UserDto? User { get; set; }
        public ClubDto? Club { get; set; }
        public ClienteDto? Cliente { get; set; }
    }
}
