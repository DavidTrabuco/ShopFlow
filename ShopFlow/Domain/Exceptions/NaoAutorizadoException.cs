namespace ShopFlow.Domain.Exceptions
{
    public class NaoAutorizadoException : Exception
    {
        public NaoAutorizadoException(string mensagem) : base(mensagem)
        {
        }
    }
}
