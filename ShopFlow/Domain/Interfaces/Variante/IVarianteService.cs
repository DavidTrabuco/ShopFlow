using ShopFlow.Domain.Entidades;

namespace ShopFlow.Domain.Interfaces.Variantes
{
    public interface IVarianteService
    {
        Task<IEnumerable<Variante>> ObterVariantesAsync();
        Task<Variante?> ObterPorIdAsync(Guid id);
        Task<Variante> CriarAsync(Guid produtoId, string sku, IEnumerable<AtributoVariante> atributos, decimal preco, decimal? precoPromocional, int pesoGramas, int quantidadeInicial);
        Task<Variante> AtualizarAsync(Guid varianteId, string sku, IEnumerable<AtributoVariante> atributos, decimal preco, decimal? precoPromocional, int pesoGramas);
        Task DesativarAsync(Guid varianteId);
        Task<Estoque> AjustarEstoqueAsync(Guid varianteId, int quantidade);
    }
}
