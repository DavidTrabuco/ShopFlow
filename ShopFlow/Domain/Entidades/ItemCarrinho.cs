namespace ShopFlow.Domain.Entidades
{
    public class ItemCarrinho
    {
        public Guid Id { get; set; }
        public int Quantidade { get; set; }



        public Guid CarrinhoId { get; set; }
        public Guid VarianteId { get; set; }
        public Carrinho? Carrinho { get; set; }
        public Variante? Variante { get; set; }
    }
}
