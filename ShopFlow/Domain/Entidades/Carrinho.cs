namespace ShopFlow.Domain.Entidades
{
    public class Carrinho
    {
        public Guid Id { get; set; }
        public string? TokenAnonimo { get; set; }
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;



        public Guid? UsuarioId { get; set; }
        public Guid? CupomId { get; set; }
        public Usuario? Usuario { get; set; }
        public ICollection<ItemCarrinho> Itens { get; set; } = new List<ItemCarrinho>();
    }
}
