using System.ComponentModel.DataAnnotations;

namespace ShopFlow.Application.DTO.Request
{
    public class CriarCategoriaRequest
    {
        [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
        [MinLength(2, ErrorMessage = "O nome da categoria deve ter pelo menos 2 caracteres.")]
        [MaxLength(80, ErrorMessage = "O nome da categoria não pode ter mais de 80 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        public Guid? CategoriaPaiId { get; set; }
    }
}
