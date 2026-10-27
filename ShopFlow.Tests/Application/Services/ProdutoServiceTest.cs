using Microsoft.EntityFrameworkCore;
using ShopFlow.Application.Service;
using ShopFlow.Domain.Entidades;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Infrastructure.Data;

namespace ShopFlow.Tests.Application.Services
{
    public class ProdutoServiceTest
    {
        private static ShopFlowDbContext CriarDbContext()
        {
            var options = new DbContextOptionsBuilder<ShopFlowDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var contexto = new ShopFlowDbContext(options);
            contexto.Database.EnsureCreated();
            return contexto;
        }

        private static Categoria AdicionarCategoria(ShopFlowDbContext contexto, string nome, bool ativa = true)
        {
            var categoria = new Categoria { Nome = nome, Ativa = ativa };
            contexto.Categorias.Add(categoria);
            contexto.SaveChanges();
            return categoria;
        }

        [Fact]
        public async Task Criar_CategoriaNaoExiste_DeveLancarNaoEncontrado()
        {
            using var db = CriarDbContext();
            var service = new ProdutoService(null!, db);

            await Assert.ThrowsAsync<NaoEncontradoException>(() => service.CriarAsync("Camisa Batman", "Camisa preta", null, Guid.NewGuid()));
        }

        [Fact]
        public async Task Criar_CategoriaDesativada_DeveLancarRegraDeNegocio()
        {
            using var db = CriarDbContext();
            var categoria = AdicionarCategoria(db, "Roupas", ativa: false);
            var service = new ProdutoService(null!, db);

            await Assert.ThrowsAsync<RegraDeNegocioException>(() => service.CriarAsync("Camisa Batman", "Camisa preta", null, categoria.Id));
        }

        [Fact]
        public async Task Criar_NomeJaUsadoPorOutroProduto_DeveLancarConflito()
        {
            using var db = CriarDbContext();
            var categoria = AdicionarCategoria(db, "Roupas");
            var service = new ProdutoService(null!, db);
            await service.CriarAsync("Camisa Batman", "Camisa preta", null, categoria.Id);

            await Assert.ThrowsAsync<ConflitoException>(() => service.CriarAsync("Camisa Batman", "Outra descrição", null, categoria.Id));
        }

        [Fact]
        public async Task Criar_NomeSoComSimbolos_DeveLancarRegraDeNegocio()
        {
            using var db = CriarDbContext();
            var categoria = AdicionarCategoria(db, "Roupas");
            var service = new ProdutoService(null!, db);

            await Assert.ThrowsAsync<RegraDeNegocioException>(() => service.CriarAsync("!!!!", "Camisa preta", null, categoria.Id));
        }

        [Fact]
        public async Task Criar_DadosValidos_DeveCriarProdutoInativo()
        {
            using var db = CriarDbContext();
            var categoria = AdicionarCategoria(db, "Roupas");
            var service = new ProdutoService(null!, db);

            var produto = await service.CriarAsync("Camisa Batman", "Camisa preta", null, categoria.Id);

            Assert.False(produto.Ativo);
            Assert.Equal("camisa-batman", produto.Slug);
            Assert.Equal(categoria.Id, produto.CategoriaId);
        }

        [Fact]
        public async Task Atualizar_ProdutoNaoExiste_DeveLancarNaoEncontrado()
        {
            using var db = CriarDbContext();
            var service = new ProdutoService(null!, db);

            await Assert.ThrowsAsync<NaoEncontradoException>(() => service.AtualizarAsync(Guid.NewGuid(), "Camisa Batman", "Camisa preta", null, Guid.NewGuid()));
        }

        [Fact]
        public async Task Atualizar_NomeDeOutroProduto_DeveLancarConflito()
        {
            using var db = CriarDbContext();
            var categoria = AdicionarCategoria(db, "Roupas");
            var service = new ProdutoService(null!, db);
            await service.CriarAsync("Camisa Batman", "Camisa preta", null, categoria.Id);
            var superman = await service.CriarAsync("Camisa Superman", "Camisa azul", null, categoria.Id);

            await Assert.ThrowsAsync<ConflitoException>(() => service.AtualizarAsync(superman.Id, "Camisa Batman", "Camisa azul", null, categoria.Id));
        }

        [Fact]
        public async Task Atualizar_MantendoOProprioNome_DevePermitir()
        {
            using var db = CriarDbContext();
            var categoria = AdicionarCategoria(db, "Roupas");
            var service = new ProdutoService(null!, db);
            var produto = await service.CriarAsync("Camisa Batman", "Camisa preta", null, categoria.Id);

            var atualizado = await service.AtualizarAsync(produto.Id, "Camisa Batman", "Camisa preta com logo dourado", null, categoria.Id);

            Assert.Equal("Camisa Batman", atualizado.Nome);
            Assert.Equal("Camisa preta com logo dourado", atualizado.Descricao);
        }

        [Fact]
        public async Task Ativar_ProdutoNaoExiste_DeveLancarNaoEncontrado()
        {
            using var db = CriarDbContext();
            var service = new ProdutoService(null!, db);

            await Assert.ThrowsAsync<NaoEncontradoException>(() => service.AtivarAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task Ativar_SemVarianteAtiva_DeveLancarRegraDeNegocio()
        {
            using var db = CriarDbContext();
            var categoria = AdicionarCategoria(db, "Roupas");
            var service = new ProdutoService(null!, db);
            var produto = await service.CriarAsync("Camisa Batman", "Camisa preta", null, categoria.Id);

            await Assert.ThrowsAsync<RegraDeNegocioException>(() => service.AtivarAsync(produto.Id));
        }

        [Fact]
        public async Task Ativar_ComVarianteAtiva_DeveAtivarProduto()
        {
            using var db = CriarDbContext();
            var categoria = AdicionarCategoria(db, "Roupas");
            var service = new ProdutoService(null!, db);
            var produto = await service.CriarAsync("Camisa Batman", "Camisa preta", null, categoria.Id);
            var varianteService = new VarianteService(null!, db);
            await varianteService.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);

            await service.AtivarAsync(produto.Id);

            var produtoNoBanco = await db.Produtos.FirstAsync(p => p.Id == produto.Id);
            Assert.True(produtoNoBanco.Ativo);
        }

        [Fact]
        public async Task Desativar_ProdutoNaoExiste_DeveLancarNaoEncontrado()
        {
            using var db = CriarDbContext();
            var service = new ProdutoService(null!, db);

            await Assert.ThrowsAsync<NaoEncontradoException>(() => service.DesativarAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task Desativar_ProdutoAtivo_DeveDesativarSemApagar()
        {
            using var db = CriarDbContext();
            var categoria = AdicionarCategoria(db, "Roupas");
            var service = new ProdutoService(null!, db);
            var produto = await service.CriarAsync("Camisa Batman", "Camisa preta", null, categoria.Id);
            var varianteService = new VarianteService(null!, db);
            await varianteService.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);
            await service.AtivarAsync(produto.Id);

            await service.DesativarAsync(produto.Id);

            var produtoNoBanco = await db.Produtos.FirstOrDefaultAsync(p => p.Id == produto.Id);
            Assert.NotNull(produtoNoBanco);
            Assert.False(produtoNoBanco.Ativo);
        }
    }
}
