namespace ShopFlow.API.Extensao
{
    public static class EsquemasAuth
    {
        // Cookie temporário: guarda os dados do Google entre o middleware (que troca o code)
        // e o endpoint de callback. Não é a sessão da aplicação.
        public const string Externo = "External";
    }
}
