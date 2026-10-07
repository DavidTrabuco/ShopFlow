using System.ComponentModel.DataAnnotations;

namespace ShopFlow.Application.DTO.Request
{
    public class CriarVarianteRequest
    {
        [Required(ErrorMessage = "O SKU é obrigatório.")]
        [MaxLength(40, ErrorMessage = "O SKU não pode ter mais de 40 caracteres.")]
        public string Sku { get; set; } = string.Empty;

        public List<AtributoVarianteRequest> Atributos { get; set; } = new();

        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
        public decimal Preco { get; set; }

        public decimal? PrecoPromocional { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "O peso não pode ser negativo.")]
        public int PesoGramas { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quantidade inicial não pode ser negativa.")]
        public int QuantidadeInicial { get; set; }
    }
}
