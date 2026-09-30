namespace ShopFlow.Domain.Exceptions
{
    // 422 - regra de negócio violada (ex.: estoque insuficiente)
    public class RegraDeNegocioException : DomainException
    {
        public RegraDeNegocioException(string mensagem) : base(mensagem)
        {
        }
    }
}
