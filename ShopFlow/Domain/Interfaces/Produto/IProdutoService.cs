using ShopFlow.Domain.Entidades;

namespace ShopFlow.Domain.Interfaces.Produtos
{
    public interface IProdutoService
    {
        Task<IEnumerable<Produto>> ListarAsync();
        Task<Produto> ObterPorIdAsync(Guid id);
        Task<Produto> CriarAsync(string nome, string descricao, string? marca, Guid categoriaId);
        Task<Produto> AtualizarAsync(Guid id, string nome, string descricao, string? marca, Guid categoriaId);
        Task AtivarAsync(Guid id);
        Task DesativarAsync(Guid id);
    }
}
