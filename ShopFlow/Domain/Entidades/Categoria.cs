namespace ShopFlow.Domain.Entidades
{
    public class Categoria
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool Ativa { get; set; } = true;



        public Guid? CategoriaPaiId { get; set; }
        public Categoria? CategoriaPai { get; set; }
        public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
        public ICollection<Categoria> Subcategorias { get; set; } = new List<Categoria>();
    }
}
