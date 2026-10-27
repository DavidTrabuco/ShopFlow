using Microsoft.EntityFrameworkCore;
using ShopFlow.Domain.Entidades;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Infrastructure.Data;
using ShopFlow.Domain.Interfaces.Carrinho;

namespace ShopFlow.Application.Service
{
    public class CarrinhoService : ICarrinhoService
    {
        private readonly ShopFlowDbContext _dbContext;

        public CarrinhoService(ShopFlowDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Carrinho> AdicionarItemAsync(Guid usuarioId, Guid varianteId, int quantidade)
        {

            var variante = await _dbContext.Variantes
                .Include(v => v.Produto)
                .Include(v => v.Estoque)
                .FirstOrDefaultAsync(v => v.Id == varianteId);

            var carrinho = await _dbContext.Carrinhos
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);


            if (variante is null || !variante.Ativa || variante.Produto is null || !variante.Produto.Ativo)
                throw new NaoEncontradoException("Variante não encontrada.");

            var itemExistente = carrinho?.Itens.FirstOrDefault(i => i.VarianteId == varianteId);
            var quantidadeFinal = (itemExistente?.Quantidade ?? 0) + quantidade;


            if (quantidadeFinal < 1 || quantidadeFinal > 10)
                throw new RegraDeNegocioException("A quantidade de cada item deve ficar entre 1 e 10.");


            var disponivel = variante.Estoque?.Disponivel ?? 0;
            if (quantidadeFinal > disponivel)
                throw new RegraDeNegocioException($"Só há {disponivel} unidade(s) disponível(is) desta variante.");


            if (carrinho is null)
            {
                carrinho = new Carrinho { UsuarioId = usuarioId };
                _dbContext.Carrinhos.Add(carrinho);
            }


            if (itemExistente is not null)
            {
                itemExistente.Quantidade = quantidadeFinal;
            }
            else
            {
                carrinho.Itens.Add(new ItemCarrinho { VarianteId = varianteId, Quantidade = quantidade });
            }

            carrinho.AtualizadoEm = DateTime.UtcNow;


            await _dbContext.SaveChangesAsync();
            return carrinho;
        }

        public async Task RemoverItemAsync(Guid usuarioId, Guid varianteId)
        {
            var carrinho = await _dbContext.Carrinhos
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            var item = carrinho?.Itens.FirstOrDefault(i => i.VarianteId == varianteId)
                ?? throw new NaoEncontradoException("Item não encontrado no carrinho.");

            carrinho!.Itens.Remove(item);
            carrinho.AtualizadoEm = DateTime.UtcNow;


            await _dbContext.SaveChangesAsync();
        }
    }
}