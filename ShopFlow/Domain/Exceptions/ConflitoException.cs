namespace ShopFlow.Domain.Exceptions
{
    // 409 - recurso já existe / estado conflitante
    public class ConflitoException : DomainException
    {
        public ConflitoException(string mensagem) : base(mensagem)
        {
        }
    }
}
