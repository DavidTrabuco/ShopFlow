using System.ComponentModel.DataAnnotations;

namespace ShopFlow.Domain.Options
{
    public class JwtOptions
    {
        public const string Secao = "Jwt";

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Required, MinLength(32)]
        public string Key { get; set; } = string.Empty;

        [Range(1, 1440)]
        public int ExpiracaoMinutos { get; set; } = 15;
    }
}
