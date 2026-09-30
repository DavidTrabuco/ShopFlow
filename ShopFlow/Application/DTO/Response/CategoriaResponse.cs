using ShopFlow.Domain.Entidades;

namespace ShopFlow.Application.DTO.Response
{
    public class CategoriaResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool Ativa { get; set; }
        public Guid? CategoriaPaiId { get; set; }

        public static CategoriaResponse De(Categoria categoria) => new()
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Slug = categoria.Slug,
            Ativa = categoria.Ativa,
            CategoriaPaiId = categoria.CategoriaPaiId
        };
    }
}
