using ShopFlow.Domain.Enums;


namespace ShopFlow.Application.DTO.Response
{
    public class AuthResponse
    {

        public string Nome { get; set; }
        public string Email { get; set; }
        public PapelUsuario PapelUsuario { get; set; }
        public string Token { get; set; }
    }
}
