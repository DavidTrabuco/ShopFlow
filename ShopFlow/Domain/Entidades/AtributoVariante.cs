namespace ShopFlow.Domain.Entidades
{
    // Um atributo de uma variante (ex.: Nome = "Cor", Valor = "Azul")
    public class AtributoVariante
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;

        public Guid VarianteId { get; set; }
        public Variante? Variante { get; set; }
    }
}
