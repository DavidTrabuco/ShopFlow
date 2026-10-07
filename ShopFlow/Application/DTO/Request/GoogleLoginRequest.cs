using System.ComponentModel.DataAnnotations;

namespace ShopFlow.Application.DTO.Request
{
    public class GoogleLoginRequest
    {
        // ID token (JWT) entregue pelo botão "Entrar com Google" no front
        [Required(ErrorMessage = "O token do Google é obrigatório.")]
        public string IdToken { get; set; } = string.Empty;
    }
}
