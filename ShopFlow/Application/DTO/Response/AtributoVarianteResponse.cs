using ShopFlow.Domain.Entidades;

namespace ShopFlow.Application.DTO.Response
{
    public class AtributoVarianteResponse
    {
        public string Nome { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;

        public static AtributoVarianteResponse De(AtributoVariante atributo) => new()
        {
            Nome = atributo.Nome,
            Valor = atributo.Valor
        };
    }
}
