using ShopFlow.Domain.Entidades;

namespace ShopFlow.Application.DTO.Response
{
    public class ProdutoResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public bool Ativo { get; set; }
        public Guid CategoriaId { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }

        // "Embrulha" a entidade no formato que o cliente pode ver
        public static ProdutoResponse De(Produto produto) => new()
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Slug = produto.Slug,
            Descricao = produto.Descricao,
            Marca = produto.Marca,
            Ativo = produto.Ativo,
            CategoriaId = produto.CategoriaId,
            CriadoEm = produto.CriadoEm,
            AtualizadoEm = produto.AtualizadoEm
        };
    }
}
