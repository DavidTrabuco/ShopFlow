using ShopFlow.Domain.Entidades;

namespace ShopFlow.Application.DTO.Response
{
    public class VarianteResponse
    {
        public Guid Id { get; set; }
        public Guid ProdutoId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public List<AtributoVarianteResponse> Atributos { get; set; } = new();
        public decimal Preco { get; set; }
        public decimal? PrecoPromocional { get; set; }
        public decimal PrecoEfetivo { get; set; }
        public int PesoGramas { get; set; }
        public bool Ativa { get; set; }

        public static VarianteResponse De(Variante variante) => new()
        {
            Id = variante.Id,
            ProdutoId = variante.ProdutoId,
            Sku = variante.Sku,
            Atributos = variante.Atributos.Select(AtributoVarianteResponse.De).ToList(),
            Preco = variante.Preco,
            PrecoPromocional = variante.PrecoPromocional,
            PrecoEfetivo = variante.PrecoEfetivo,
            PesoGramas = variante.PesoGramas,
            Ativa = variante.Ativa
        };
    }
}
