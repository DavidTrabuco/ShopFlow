using ShopFlow.Application.Common;

namespace ShopFlow.Tests.Application.Common;

public class SlugTests
{
    [Theory]
    [InlineData("Categoria de Teste", "categoria-de-teste")]
    [InlineData("Camisa - Polo", "camisa-polo")]
    [InlineData("Camisa/Polo", "camisa-polo")]
    [InlineData("Tênis & Meias", "tenis-meias")]
    [InlineData(" -Promo- ", "promo")]
    public void GerarSlug_DeveRetornarSlugCorreto(string entrada, string esperado)
    {
        var slug = Slug.GerarSlug(entrada);
        Assert.Equal(esperado, slug);
    }
}
