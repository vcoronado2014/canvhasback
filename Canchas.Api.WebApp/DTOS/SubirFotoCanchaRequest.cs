namespace Canchas.Api.WebApp.DTOS
{
    public class SubirFotoCanchaRequest
    {
        public IFormFile Archivo { get; set; } = null!;
        public bool EsPrincipal { get; set; } = false;
        public int Orden { get; set; } = 0;
    }
}
