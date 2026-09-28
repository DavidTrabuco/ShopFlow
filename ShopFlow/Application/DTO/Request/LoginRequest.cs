using System.ComponentModel.DataAnnotations;

namespace ShopFlow.Application.DTO.Request
{
    public class LoginRequest
    {

        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [MaxLength(180)]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "O campo Email deve ser um endereço de email válido.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        [MinLength(10, ErrorMessage = "A senha deve ter pelo menos 10 caracteres.")]
        [MaxLength(72, ErrorMessage = "A senha não pode ter mais de 72 caracteres.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "A senha precisa ter ao menos uma letra e um número.")]
        public string Senha { get; set; }
    }
}
