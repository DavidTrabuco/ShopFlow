namespace ShopFlow.Domain.Entidades
{
    public class ImagemProduto
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public int Ordem { get; set; }



        public Guid ProdutoId { get; set; }
        public Guid? VarianteId { get; set; }
        public Produto? Produto { get; set; }
        public Variante? Variante { get; set; }
    }
}
