namespace ShopFlow.Domain.Exceptions
{
    // 401 - credenciais inválidas
    public class NaoAutorizadoException : DomainException
    {
        public NaoAutorizadoException(string mensagem) : base(mensagem)
        {
        }
    }
}
