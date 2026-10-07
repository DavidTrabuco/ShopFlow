namespace ShopFlow.Domain.Interfaces
{
    // Dados que o Google garante sobre quem logou
    public record GoogleUsuarioInfo(string GoogleId, string Email, bool EmailVerificado, string Nome);

    public interface IGoogleTokenValidator
    {
        // Valida assinatura, validade e audience do ID token. Token inválido → NaoAutorizadoException.
        Task<GoogleUsuarioInfo> ValidarAsync(string idToken);
    }
}
