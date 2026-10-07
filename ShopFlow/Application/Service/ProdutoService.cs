using Microsoft.EntityFrameworkCore;
using ShopFlow.Application.Common;
using ShopFlow.Domain.Entidades;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Domain.Interfaces.Produtos;
using ShopFlow.Infrastructure.Data;

namespace ShopFlow.Application.Service
{
    // Todo método segue o mesmo roteiro: 1. BUSCAR -> 2. CHECAR (regras) -> 3. MONTAR -> 4. SALVAR
    public class ProdutoService : IProdutoService
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly ShopFlowDbContext _db;

        public ProdutoService(IProdutoRepository produtoRepository, ShopFlowDbContext db)
        {
            _produtoRepository = produtoRepository;
            _db = db;
        }

       
        public async Task<IEnumerable<Produto>> ListarAsync()
        {
            return await _produtoRepository.ObterProdutosAsync();
        }

        public async Task<Produto> ObterPorIdAsync(Guid id)
        {
           
            return await _produtoRepository.ObterProdutoPorIdAsync(id)
                ?? throw new NaoEncontradoException("Produto não encontrado.");
        }

      
        public async Task<Produto> CriarAsync(string nome, string descricao, string? marca, Guid categoriaId)
        {
            
            var slug = GerarSlug(nome);

            // 2. CHECAR
            // RN-01: o slug não se repete entre produtos
            if (await _db.Produtos.AnyAsync(p => p.Slug == slug))
                throw new ConflitoException("Já existe um produto com esse nome.");

            // RN-02: a categoria existe e está ativa
            await ValidarCategoriaAsync(categoriaId);

            // 3. MONTAR
            var produto = new Produto
            {
                Id = Guid.NewGuid(),
                Nome = nome.Trim(),
                Slug = slug,
                Descricao = descricao.Trim(),
                Marca = string.IsNullOrWhiteSpace(marca) ? null : marca.Trim(),
                CategoriaId = categoriaId,
                Ativo = false, // RN-03: todo produto nasce inativo (rascunho)
                CriadoEm = DateTime.UtcNow,
                AtualizadoEm = DateTime.UtcNow
            };

            // 4. SALVAR
            _db.Produtos.Add(produto);
            await _db.SaveChangesAsync();

            return produto;
        }

        // CAT-02
        public async Task<Produto> AtualizarAsync(Guid id, string nome, string descricao, string? marca, Guid categoriaId)
        {
            // 1. BUSCAR (pelo EF, porque vou alterar a entidade)
            var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NaoEncontradoException("Produto não encontrado."); 

            var slug = GerarSlug(nome);

            // 2. CHECAR
            // RN-01: slug único, ignorando o próprio produto (senão ele conflitaria consigo mesmo)
            if (await _db.Produtos.AnyAsync(p => p.Slug == slug && p.Id != id))
                throw new ConflitoException("Já existe um produto com esse nome.");

            // RN-02: a categoria existe e está ativa
            await ValidarCategoriaAsync(categoriaId);

            // 3. MONTAR
            produto.Nome = nome.Trim();
            produto.Slug = slug;
            produto.Descricao = descricao.Trim();
            produto.Marca = string.IsNullOrWhiteSpace(marca) ? null : marca.Trim();
            produto.CategoriaId = categoriaId;
            produto.AtualizadoEm = DateTime.UtcNow; // RN-06

            // 4. SALVAR
            await _db.SaveChangesAsync();

            return produto;
        }

        // CAT-05
        public async Task AtivarAsync(Guid id)
        {
            // 1. BUSCAR
            var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NaoEncontradoException("Produto não encontrado.");

            // 2. CHECAR
           
            var temVarianteAtiva = await _db.Variantes.AnyAsync(v => v.ProdutoId == id && v.Ativa);
            if (!temVarianteAtiva)
                throw new RegraDeNegocioException("Cadastre pelo menos uma variante ativa antes de ativar o produto.");

            // 3. MONTAR
            produto.Ativo = true;
            produto.AtualizadoEm = DateTime.UtcNow; // RN-06

            // 4. SALVAR
            await _db.SaveChangesAsync();
        }

        // CAT-06
        public async Task DesativarAsync(Guid id)
        {
            // 1. BUSCAR
            var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NaoEncontradoException("Produto não encontrado.");

           

            // 3. MONTAR
            produto.Ativo = false; 
            produto.AtualizadoEm = DateTime.UtcNow; // RN-06

            // 4. SALVAR
            await _db.SaveChangesAsync();
        }

      
        private static string GerarSlug(string nome)
        {
            var slug = Slug.GerarSlug(nome);

           
            if (string.IsNullOrEmpty(slug))
                throw new RegraDeNegocioException("O nome do produto precisa ter letras ou números.");

            return slug;
        }

       
        private async Task ValidarCategoriaAsync(Guid categoriaId)
        {
            var categoria = await _db.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == categoriaId)
                ?? throw new NaoEncontradoException("Categoria não encontrada.");

            if (!categoria.Ativa)
                throw new RegraDeNegocioException("A categoria está desativada.");
        }
    }
}
