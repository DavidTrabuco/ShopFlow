using System.ComponentModel.DataAnnotations;

namespace ShopFlow.Domain.Options
{
    
    public class GoogleAuthOptions
    {
        public const string Secao = "Google";

        [Required(ErrorMessage = "Google:ClientId não configurado. Rode: dotnet user-secrets set \"Google:ClientId\" \"<valor>\"")]
        public string ClientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Google:ClientSecret não configurado. Rode: dotnet user-secrets set \"Google:ClientSecret\" \"<valor>\"")]
        public string ClientSecret { get; set; } = string.Empty;
    }
}
