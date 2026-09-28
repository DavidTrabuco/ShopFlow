using ShopFlow.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace ShopFlow.Application.DTO.Request
{
    public class RegistrarRequest
    {

        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [MaxLength(120)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [MaxLength(120)]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "O campo Email deve ser um endereço de email válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        [MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
        [MaxLength(72, ErrorMessage = "A senha não pode ter mais de 72 caracteres.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "A senha precisa ter ao menos uma letra e um número.")]
        public string Senha { get; set; } = string.Empty;

    }
}
