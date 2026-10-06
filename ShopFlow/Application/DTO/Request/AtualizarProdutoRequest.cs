using System.ComponentModel.DataAnnotations;

namespace ShopFlow.Application.DTO.Request
{
    public class AtualizarProdutoRequest
    {
        [Required(ErrorMessage = "O nome do produto é obrigatório.")]
        [MinLength(2, ErrorMessage = "O nome do produto deve ter pelo menos 2 caracteres.")]
        [MaxLength(150, ErrorMessage = "O nome do produto não pode ter mais de 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição do produto é obrigatória.")]
        public string Descricao { get; set; } = string.Empty;

        [MaxLength(80, ErrorMessage = "A marca não pode ter mais de 80 caracteres.")]
        public string? Marca { get; set; }

        // Se vier vazio ou inexistente, o service responde 404 "Categoria não encontrada" (RN-02)
        public Guid CategoriaId { get; set; }
    }
}
