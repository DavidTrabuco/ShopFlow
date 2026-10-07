using System.ComponentModel.DataAnnotations;

namespace ShopFlow.Domain.Options
{
    public class GoogleOptions
    {
        public const string Secao = "Google";

        // Client ID do OAuth (Google Cloud). Não é segredo, mas fica fora do código:
        // user-secrets localmente, variável de ambiente Google__ClientId no Render.
        [Required]
        public string ClientId { get; set; } = string.Empty;
    }
}
