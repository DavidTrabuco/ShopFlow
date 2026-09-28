namespace ShopFlow.Domain.Entidades
{
    public class Estoque
    {
        public Guid VarianteId { get; set; }
        public int QuantidadeFisica { get; set; }
        public int QuantidadeReservada { get; set; }
        public int Disponivel => QuantidadeFisica - QuantidadeReservada;
        public uint Versao { get; set; }



        public Variante? Variante { get; set; }
    }
}
