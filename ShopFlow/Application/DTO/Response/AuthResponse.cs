using ShopFlow.Domain.Entidades;
using ShopFlow.Domain.Enums;

namespace ShopFlow.Application.DTO.Response
{
    public class AuthResponse
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public PapelUsuario PapelUsuario { get; set; }

        public static AuthResponse De(Usuario usuario) => new()
        {
            Nome = usuario.Nome,
            Email = usuario.Email,
            PapelUsuario = usuario.Papel
        };
    }
}
