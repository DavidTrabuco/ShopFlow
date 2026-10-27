using ShopFlow.Domain.Entidades;
using System;
using System.Threading.Tasks;
namespace ShopFlow.Domain.Interfaces.Carrinho
{
    public interface ICarrinhoService
    {

        public Task AdicionarItemAsync(Guid usuarioId, Guid varianteId, int quantidade);
        public Task RemoverItemAsync(Guid usuarioId, Guid varianteId);

        

    }
}
