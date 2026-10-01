using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ShopFlow.Application.Common
{
    public static class Slug
    {
        public static string GerarSlug(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O nome não pode ser vazio.", nameof(nome));
            // Remove acentos e caracteres especiais
            var normalizedString = nome.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            var slug = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            // Substitui espaços por hífens e remove caracteres inválidos
            slug = Regex.Replace(slug.ToLowerInvariant(), @"[^a-z0-9]+", "-");
            slug = slug.Trim('-');
            return slug;
        }
    }
}
