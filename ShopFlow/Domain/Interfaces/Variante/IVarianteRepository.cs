using ShopFlow.Domain.Entidades;

namespace ShopFlow.Domain.Interfaces.Variantes
{
    public interface IVarianteRepository
    {

        Task<IEnumerable<Variante>> ObterVariantesAsync();
        Task<Variante?> ObterPorIdAsync(Guid id);




    }
}
