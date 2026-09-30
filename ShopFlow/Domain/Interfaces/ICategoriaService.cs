using ShopFlow.Domain.Entidades;

namespace ShopFlow.Domain.Interfaces
{
    public interface ICategoriaService
    {
        Task<IEnumerable<Categoria>> ListarAsync();
        Task<Categoria> ObterPorIdAsync(Guid id);
        Task<Categoria> CriarAsync(string nome, Guid? categoriaPaiId);
        Task<Categoria> AtualizarAsync(Guid id, string nome, Guid? categoriaPaiId);
        Task DesativarAsync(Guid id);
    }
}
