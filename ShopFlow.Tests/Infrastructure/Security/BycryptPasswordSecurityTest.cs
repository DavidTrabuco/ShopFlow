using System;
using System.Collections.Generic;
using System.Text;
using ShopFlow.Infrastructure.Security;
using Xunit;

namespace ShopFlow.Tests.Infrastructure.Security
{
    public class BycryptPasswordSecurityTest
    {

        private readonly BcryptPasswordHasher _hasher = new();

        private readonly string Senha = "MinhaSenha123!";



        [Fact]
        public void HashNaoEIgualAoPassword()
        {
            var hashedPassword = _hasher.Gerar(Senha);
            var isValid = _hasher.Verificar(Senha, hashedPassword);
            Assert.NotEqual(Senha, hashedPassword);
        }

        [Fact]
        public void VerificarComSenhaCorreta()
        {
            var hashedPassword = _hasher.Gerar(Senha);
            var isValid = _hasher.Verificar(Senha, hashedPassword);
            Assert.True(isValid);
        }

        [Fact]
        public void VerificarComSenhaIncorreta()
        {
            var hashedPassword = _hasher.Gerar(Senha);
            var isValid = _hasher.Verificar("SenhaIncorreta", hashedPassword);
            Assert.False(isValid);
        }

        [Fact]
        public void GerarDuasSenhasIguais()
        {
            var hashedPassword1 = _hasher.Gerar(Senha);
            var hashedPassword2 = _hasher.Gerar(Senha);
            Assert.NotEqual(hashedPassword1, hashedPassword2);
        }
    }
}
