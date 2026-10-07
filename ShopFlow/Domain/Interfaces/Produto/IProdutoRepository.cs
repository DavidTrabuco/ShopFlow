using ShopFlow.Domain.Entidades;

namespace ShopFlow.Domain.Interfaces.Produtos
{
    public interface IProdutoRepository
    {
        
        public Task<IEnumerable<Produto>> ObterProdutosAsync();
        public Task<Produto?> ObterProdutoPorIdAsync(Guid id);
    }
}
