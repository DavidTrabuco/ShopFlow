using System.ComponentModel.DataAnnotations;

namespace ShopFlow.Application.DTO.Request
{
    public class AtributoVarianteRequest
    {
        [Required(ErrorMessage = "O nome do atributo é obrigatório.")]
        [MaxLength(50, ErrorMessage = "O nome do atributo não pode ter mais de 50 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O valor do atributo é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O valor do atributo não pode ter mais de 100 caracteres.")]
        public string Valor { get; set; } = string.Empty;
    }
}
