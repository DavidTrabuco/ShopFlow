using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ShopFlow.Application.Service;
using ShopFlow.Domain.Entidades;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Infrastructure.Data;

namespace ShopFlow.Tests.Application.Services
{
    public class VarianteServiceTest
    {
        // Cada teste ganha um banco novo e vazio na memória
        private static ShopFlowDbContext CriarDbContext()
        {
            var options = new DbContextOptionsBuilder<ShopFlowDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ShopFlowDbContext(options);
        }

        [Fact]
        public async Task Criar_SkuJaExistente_DeveLancarConflito()
        {
            // ARRANGE
            using var dbContext = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            dbContext.AddRange(categoria, produto);
            await dbContext.SaveChangesAsync();
            var service = new VarianteService(null!, dbContext);
            await service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);

            // ACT + ASSERT
            await Assert.ThrowsAsync<ConflitoException>(() => service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 15));
        }




        [Fact]
        public async Task Criar_ProdutoNaoExistente_DeveLancarNaoEncontrado()
        {
            // ARRANGE
            using var dbContext = CriarDbContext();
            var service = new VarianteService(null!, dbContext);



            // ASSERT
            await Assert.ThrowsAsync<NaoEncontradoException>(() => service.CriarAsync(Guid.NewGuid(), "CAM-01", [], 100m, null, 200, 10));

        }

        [Fact]
        public async Task Criar_QuantidadeNegativa_DeveLancarValidacao()
        {

            //Arrange 
            using var dbContext = CriarDbContext();

            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            dbContext.AddRange(categoria, produto);
            await dbContext.SaveChangesAsync();

            var service = new VarianteService(null!, dbContext);

            await Assert.ThrowsAsync<ValidacaoException>(() => service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, -5));


        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async Task Criar_PrecoInvalido_DeveLancarvalidacao(decimal preco)
        {
            //Arrange 
            using var dbContext = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            dbContext.AddRange(categoria, produto);
            await dbContext.SaveChangesAsync();

            //Act + Assert
            var service = new VarianteService(null!, dbContext);
            await Assert.ThrowsAsync<ValidacaoException>(() => service.CriarAsync(produto.Id, "CAM-01", [], preco, null, 200, 10));
        }


        [Theory]
        [InlineData(100)]
        [InlineData(150)]
        [InlineData(0)]
        public async Task Criar_PromocionalInvalido_DeveLancarValidacao(decimal precoPromocional)
        {

            var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();
            var service = new VarianteService(null!, db);
            await Assert.ThrowsAsync<ValidacaoException>(() => service.CriarAsync(produto.Id, "CAM-01", [], 100m, precoPromocional, 200, 10));
        }

        [Fact]
        public async Task Criar_DadosValidos_DeveCriarVarianteAtivaComEstoque()
        {
            var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();

            var service = new VarianteService(null!, db);
            var variante = await service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);

            var estoque = await db.Estoques.FirstOrDefaultAsync(e => e.VarianteId == variante.Id);

            Assert.NotNull(variante);
            Assert.True(variante.Ativa);
            Assert.NotNull(estoque);
            Assert.Equal(10, estoque.QuantidadeFisica);
            Assert.Equal(0, estoque.QuantidadeReservada);
        }


        [Fact]
        public async Task Criar_AtributosSemValor_DeveLancarValidacao()
        {
            //Act 
            var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();

            //Act 
            var service = new VarianteService(null!, db);
            var atributosInvalidos = new List<AtributoVariante>
            {
                new AtributoVariante { Nome = "Cor", Valor = "Azul" },
                new AtributoVariante { Nome = "Tamanho", Valor = "" }
            };

            //Assert 
            await Assert.ThrowsAsync<ValidacaoException>(() => service.CriarAsync(produto.Id, "CAM-01", atributosInvalidos, 100m, null, 200, 10));
        }

        [Fact]
        public async Task Criar_AtributoRepetido_deveLancarValidacao()
        {
            //Arrange 
            var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();
            //Act 
            var service = new VarianteService(null!, db);
            var atributosInvalidos = new List<AtributoVariante>
            {
                new AtributoVariante { Nome = "Cor", Valor = "Azul" },
                new AtributoVariante { Nome = "Cor", Valor = "Vermelho" }
            };
            //Assert 
            await Assert.ThrowsAsync<ValidacaoException>(() => service.CriarAsync(produto.Id, "CAM-01", atributosInvalidos, 100m, null, 200, 10));
        }


        [Fact]
        public async Task Atualizar_VarianteInexistente_deveLancarNaoEncontrado()
        {
            //Arrange 
            var db = CriarDbContext();
            var service = new VarianteService(null!, db);
            //Act + Assert
            await Assert.ThrowsAsync<NaoEncontradoException>(() => service.AtualizarAsync(Guid.NewGuid(), "CAM-01", [], 100m, null, 0));


        }

        [Fact]
        public async Task Atualizar_MantendoOProprioSku_DevePermitir()
        {
            var db = CriarDbContext();

            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };

            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();
            var service = new VarianteService(null!, db);
            var varianteA = await service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);
            var varianteAtualizado = await service.AtualizarAsync(varianteA.Id, "CAM-01", [], 150m, null, 200);


            Assert.Equal("CAM-01", varianteAtualizado.Sku);
            Assert.Equal(150m, varianteAtualizado.Preco);
        }

        [Fact]
        public async Task Atualizar_AtributosSemValor_DeveLancarValidacao()
        {
            var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();
            var service = new VarianteService(null!, db);
            var varianteA = await service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);
            var atributosInvalidos = new List<AtributoVariante>
            {
                new AtributoVariante { Nome = "Cor", Valor = "" }
            };
            await Assert.ThrowsAsync<ValidacaoException>(() => service.AtualizarAsync(varianteA.Id, "CAM-01", atributosInvalidos, 100m, null, 200));
        }

        [Fact]
        public async Task Atualizar_SkuDeOutraVariante_deveLancarConflito()
        {
            var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();
            var service = new VarianteService(null!, db);
            var varianteA = await service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);
            var varianteB = await service.CriarAsync(produto.Id, "CAM-02", [], 100m, null, 200, 10);
            await Assert.ThrowsAsync<ConflitoException>(() => service.AtualizarAsync(varianteA.Id, "CAM-02", [], 100m, null, 200));


        }

        [Fact]
        public async Task Atualizar_PrecoInvalido_DeveLancarValidacao()
        {
            var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();
            var service = new VarianteService(null!, db);
            var varianteA = await service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);
            await Assert.ThrowsAsync<ValidacaoException>(() => service.AtualizarAsync(varianteA.Id, "CAM-01", [], -50m, null, 200));
        }



        [Fact]
        public async Task Atualizar_TrocandoAtributos_DeveSubstituirOsAntigos()
        {
            // ARRANGE: variante A cadastrada como "Cor: Azul"
            using var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();
            var service = new VarianteService(null!, db);
            var atributosAntigos = new List<AtributoVariante>
            {
                new AtributoVariante { Nome = "Cor", Valor = "Azul" }
            };
            var varianteA = await service.CriarAsync(produto.Id, "CAM-01", atributosAntigos, 100m, null, 200, 10);

            // ACT: o admin corrige para "Cor: Verde"
            var atributosNovos = new List<AtributoVariante>
            {
                new AtributoVariante { Nome = "Cor", Valor = "Verde" }
            };
            await service.AtualizarAsync(varianteA.Id, "CAM-01", atributosNovos, 100m, null, 200);

            // ASSERT: no banco sobrou um atributo só, e ele é "Verde"
            var atributos = await db.AtributosVariante.Where(a => a.VarianteId == varianteA.Id).ToListAsync();
            var atributo = Assert.Single(atributos);
            Assert.Equal("Verde", atributo.Valor);
        }
    

        // ===================== DesativarAsync =====================

        [Fact]
        public async Task Desativar_VarianteInexistente_DeveLancarNaoEncontrado()
        {
            // ARRANGE: banco vazio, nenhuma variante
            using var db = CriarDbContext();
            var service = new VarianteService(null!, db);

            // ACT + ASSERT
            await Assert.ThrowsAsync<NaoEncontradoException>(() => service.DesativarAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task Desativar_UltimaAtivaDeProdutoAtivo_DeveLancarConflito()
        {
            // ARRANGE: produto À VENDA com uma única variante
            using var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();
            var service = new VarianteService(null!, db);
            var varianteA = await service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);

            // ACT + ASSERT: desativar a única variante deixaria o produto à venda sem nada para vender
            await Assert.ThrowsAsync<ConflitoException>(() => service.DesativarAsync(varianteA.Id));
        }

        [Fact]
        public async Task Desativar_UltimaAtivaDeProdutoInativo_DevePermitir()
        {
            // ARRANGE: produto FORA de venda (Ativo = false) com uma única variante
            using var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = false };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();
            var service = new VarianteService(null!, db);
            var varianteA = await service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);

            // ACT
            await service.DesativarAsync(varianteA.Id);

            // ASSERT: como o produto não está à venda, pode desativar
            var varianteNoBanco = await db.Variantes.FirstAsync(v => v.Id == varianteA.Id);
            Assert.False(varianteNoBanco.Ativa);
        }

        [Fact]
        public async Task Desativar_ComOutraVarianteAtiva_DevePermitir()
        {
            // ARRANGE: produto à venda com DUAS variantes
            using var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();
            var service = new VarianteService(null!, db);
            var varianteA = await service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);
            await service.CriarAsync(produto.Id, "CAM-02", [], 100m, null, 200, 10);

            // ACT: desativa a A; a B continua à venda
            await service.DesativarAsync(varianteA.Id);

            // ASSERT
            var varianteNoBanco = await db.Variantes.FirstAsync(v => v.Id == varianteA.Id);
            Assert.False(varianteNoBanco.Ativa);
        }

        [Fact]
        public async Task Desativar_DeveManterALinhaNoBanco()
        {
            // ARRANGE: duas variantes, para a regra da última ativa não atrapalhar
            using var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();
            var service = new VarianteService(null!, db);
            var varianteA = await service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);
            await service.CriarAsync(produto.Id, "CAM-02", [], 100m, null, 200, 10);

            // ACT
            await service.DesativarAsync(varianteA.Id);

            // ASSERT: desativar NÃO apaga (soft delete): a variante continua no banco
            var varianteNoBanco = await db.Variantes.FirstOrDefaultAsync(v => v.Id == varianteA.Id);
            Assert.NotNull(varianteNoBanco);
        }

        [Fact]
        public async Task Desativar_JaInativa_NaoDeveLancar()
        {
            // ARRANGE: duas variantes, e a A já desativada uma vez
            using var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();
            var service = new VarianteService(null!, db);
            var varianteA = await service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);
            await service.CriarAsync(produto.Id, "CAM-02", [], 100m, null, 200, 10);
            await service.DesativarAsync(varianteA.Id);

            // ACT: desativar de novo (o admin clicou duas vezes)
            await service.DesativarAsync(varianteA.Id);

            // ASSERT: se a linha de cima lançasse exceção, o teste já teria caído; ela continua inativa
            var varianteNoBanco = await db.Variantes.FirstAsync(v => v.Id == varianteA.Id);
            Assert.False(varianteNoBanco.Ativa);
        }

        // ===================== AjustarEstoqueAsync =====================

        [Fact]
        public async Task AjustarEstoque_VarianteInexistente_DeveLancarNaoEncontrado()
        {
            // ARRANGE: banco vazio
            using var db = CriarDbContext();
            var service = new VarianteService(null!, db);

            // ACT + ASSERT
            await Assert.ThrowsAsync<NaoEncontradoException>(() => service.AjustarEstoqueAsync(Guid.NewGuid(), 10));
        }

        [Fact]
        public async Task AjustarEstoque_Negativo_DeveLancarRegraDeNegocio()
        {
            // ARRANGE: uma variante com 10 unidades
            using var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();
            var service = new VarianteService(null!, db);
            var varianteA = await service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);

            // ACT + ASSERT
            await Assert.ThrowsAsync<RegraDeNegocioException>(() => service.AjustarEstoqueAsync(varianteA.Id, -1));
        }

        [Fact]
        public async Task AjustarEstoque_MenorQueReservado_DeveLancarRegraDeNegocio()
        {
            // ARRANGE: 10 unidades, das quais 5 estão reservadas por pedidos
            using var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();
            var service = new VarianteService(null!, db);
            var varianteA = await service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);

            var estoque = await db.Estoques.FirstAsync(e => e.VarianteId == varianteA.Id);
            estoque.QuantidadeReservada = 5;
            await db.SaveChangesAsync();

            // ACT + ASSERT: não dá para ter só 3 no estoque se 5 já foram prometidas
            await Assert.ThrowsAsync<RegraDeNegocioException>(() => service.AjustarEstoqueAsync(varianteA.Id, 3));
        }

        [Fact]
        public async Task AjustarEstoque_IgualAoReservado_DevePermitir()
        {
            // ARRANGE: 10 unidades, 5 reservadas
            using var db = CriarDbContext();
            var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Camisas", Slug = "camisas" };
            var produto = new Produto { Id = Guid.NewGuid(), Nome = "CamisaBatman", Slug = "camisa", CategoriaId = categoria.Id, Ativo = true };
            db.AddRange(categoria, produto);
            await db.SaveChangesAsync();
            var service = new VarianteService(null!, db);
            var varianteA = await service.CriarAsync(produto.Id, "CAM-01", [], 100m, null, 200, 10);

            var estoque = await db.Estoques.FirstAsync(e => e.VarianteId == varianteA.Id);
            estoque.QuantidadeReservada = 5;
            await db.SaveChangesAsync();

            // ACT: ajustar para exatamente 5 (o limite da regra)
            var estoqueAjustado = await service.AjustarEstoqueAsync(varianteA.Id, 5);

            // ASSERT: aceitou; tudo o que existe está reservado, e nada fica disponível
            Assert.Equal(5, estoqueAjustado.QuantidadeFisica);
            Assert.Equal(0, estoqueAjustado.Disponivel);
        }
    }
}
