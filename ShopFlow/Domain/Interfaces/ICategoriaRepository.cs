using ShopFlow.Domain.Entidades;

namespace ShopFlow.Domain.Interfaces
{
    public interface ICategoriaRepository
    {


        Task<IEnumerable<Categoria>> ObterCategoriasAsync();
        Task<Categoria?> ObterCategoriaPorIdAsync(Guid id);
        Task<Categoria?> ObterCategoriaPorSlugAsync(string slug);
        Task<IEnumerable<Categoria>> ObterSubcategoriasAsync(Guid categoriaPaiId);
    }
}
