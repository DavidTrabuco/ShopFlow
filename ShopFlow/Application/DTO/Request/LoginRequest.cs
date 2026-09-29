using System.ComponentModel.DataAnnotations;

namespace ShopFlow.Application.DTO.Request
{
    public class LoginRequest
    {

        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [MaxLength(180)]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "O campo Email deve ser um endereço de email válido.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        [MaxLength(72, ErrorMessage = "A senha não pode ter mais de 72 caracteres.")]
        public string Senha { get; set; } = string.Empty;
    }
}
