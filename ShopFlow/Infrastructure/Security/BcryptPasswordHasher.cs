using ShopFlow.Domain.Interfaces;

namespace ShopFlow.Infrastructure.Security
{
    public class BcryptPasswordHasher : IPasswordHasher
    {
        public string Gerar(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verificar(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
