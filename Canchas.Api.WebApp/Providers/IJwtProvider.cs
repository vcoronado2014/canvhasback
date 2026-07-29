using Canchas.Api.WebApp.DTOS;

namespace Canchas.Api.WebApp.Providers
{
    public interface IJwtProvider
    {
        string Generate(AuthUserDto usuario);
    }
}
