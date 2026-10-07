using ShopFlow.Domain.Entidades;
using ShopFlow.Domain.Enums;

namespace ShopFlow.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<Usuario> RegistrarAsync(string nome, string email, string senha);
        Task<Usuario> LoginAsync(string email, string senha);
        Task<Usuario> ObterOuCriarViaGoogleAsync(string googleId, string email, string nome, bool emailVerificado);

        Task CriarSessaoAsync(Guid usuarioId, string tokenHash);
        Task<Usuario?> ValidarSessaoAsync(string tokenHash);
        Task EncerrarSessaoAsync(string tokenHash);

        Task DeletarContaAsync(Guid usuarioId);

        Task AlterarPapelUsuarioAsync(Guid usuarioId, PapelUsuario novoPapel);

    }
}
