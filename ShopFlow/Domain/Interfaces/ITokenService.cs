using ShopFlow.Domain.Entidades;

namespace ShopFlow.Domain.Interfaces
{
    public interface ITokenService
    {
        string GerarToken(Usuario usuario);
        string GerarTokenSessao();
        string HashTokenSessao(string tokenSessao);
    }
}
