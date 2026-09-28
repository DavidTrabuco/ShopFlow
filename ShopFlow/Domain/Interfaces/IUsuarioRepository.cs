using ShopFlow.Domain.Entidades;

namespace ShopFlow.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObterPorEmailAsync(string email);
        Task<bool> EmailExisteAsync(string email);
        
    }
}
