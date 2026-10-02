using ShopFlow.Domain.Entidades;

namespace ShopFlow.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<Usuario> RegistrarAsync(string nome, string email, string senha);
        Task<Usuario> LoginAsync(string email, string senha);

        Task CriarSessaoAsync(Guid usuarioId, string tokenHash);
        Task<Usuario?> ValidarSessaoAsync(string tokenHash);
        Task EncerrarSessaoAsync(string tokenHash);

        Task DeletarContaAsync(Guid usuarioId);

    }
}
