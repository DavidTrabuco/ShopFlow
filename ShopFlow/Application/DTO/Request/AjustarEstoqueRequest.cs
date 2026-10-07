using System.ComponentModel.DataAnnotations;

namespace ShopFlow.Application.DTO.Request
{
    public class AjustarEstoqueRequest
    {
        // Novo valor da quantidade física (não é uma variação)
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade não pode ser negativa.")]
        public int Quantidade { get; set; }
    }
}
