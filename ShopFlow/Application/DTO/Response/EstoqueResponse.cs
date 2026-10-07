using ShopFlow.Domain.Entidades;

namespace ShopFlow.Application.DTO.Response
{
    public class EstoqueResponse
    {
        public Guid VarianteId { get; set; }
        public int QuantidadeFisica { get; set; }
        public int QuantidadeReservada { get; set; }
        public int Disponivel { get; set; }

        public static EstoqueResponse De(Estoque estoque) => new()
        {
            VarianteId = estoque.VarianteId,
            QuantidadeFisica = estoque.QuantidadeFisica,
            QuantidadeReservada = estoque.QuantidadeReservada,
            Disponivel = estoque.Disponivel
        };
    }
}
