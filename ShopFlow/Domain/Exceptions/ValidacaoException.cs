namespace ShopFlow.Domain.Exceptions
{
    // 400 - dado de entrada inválido (ex.: preço promocional maior que o preço)
    public class ValidacaoException : DomainException
    {
        public ValidacaoException(string mensagem) : base(mensagem)
        {
        }
    }
}
