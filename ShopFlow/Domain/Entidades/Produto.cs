namespace ShopFlow.Domain.Entidades
{
    public class Produto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;



        public Guid CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
        public ICollection<Variante> Variantes { get; set; } = new List<Variante>();
        public ICollection<ImagemProduto> Imagens { get; set; } = new List<ImagemProduto>();
    }
}
