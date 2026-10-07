namespace ShopFlow.Domain.Entidades
{
    public class Variante
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public List<AtributoVariante> Atributos { get; set; } = new();
        public decimal Preco { get; set; }
        public decimal? PrecoPromocional { get; set; }
        public decimal PrecoEfetivo => PrecoPromocional ?? Preco;
        public int PesoGramas { get; set; }
        public bool Ativa { get; set; } = true;



        public Guid ProdutoId { get; set; }
        public Produto? Produto { get; set; }
        public Estoque? Estoque { get; set; }
    }
}
