using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ShopFlow.Domain.Entidades;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Domain.Interfaces;
using ShopFlow.Infrastructure.Data;

namespace ShopFlow.Application.Service
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly ShopFlowDbContext _db;

        public CategoriaService(ICategoriaRepository categoriaRepository, ShopFlowDbContext db)
        {
            _categoriaRepository = categoriaRepository;
            _db = db;
        }

        public async Task<IEnumerable<Categoria>> ListarAsync()
        {
            return await _categoriaRepository.ObterCategoriasAsync();
        }

        public async Task<Categoria> ObterPorIdAsync(Guid id)
        {
            return await _categoriaRepository.ObterCategoriaPorIdAsync(id)
                ?? throw new NaoEncontradoException("Categoria não encontrada.");
        }

        public async Task<Categoria> CriarAsync(string nome, Guid? categoriaPaiId)
        {
            var slug = GerarSlug(nome);

            if (await _categoriaRepository.ObterCategoriaPorSlugAsync(slug) is not null)
                throw new ConflitoException("Já existe uma categoria com esse nome.");

            if (categoriaPaiId is not null)
                await ValidarCategoriaPaiAsync(categoriaPaiId.Value);

            var categoria = new Categoria
            {
                Id = Guid.NewGuid(),
                Nome = nome.Trim(),
                Slug = slug,
                CategoriaPaiId = categoriaPaiId,
                Ativa = true
            };

            _db.Categorias.Add(categoria);
            await _db.SaveChangesAsync();

            return categoria;
        }

        public async Task<Categoria> AtualizarAsync(Guid id, string nome, Guid? categoriaPaiId)
        {
            var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NaoEncontradoException("Categoria não encontrada.");

            var slug = GerarSlug(nome);
            var mesmoSlug = await _categoriaRepository.ObterCategoriaPorSlugAsync(slug);
            if (mesmoSlug is not null && mesmoSlug.Id != id)
                throw new ConflitoException("Já existe uma categoria com esse nome.");

            if (categoriaPaiId is not null)
            {
                if (categoriaPaiId == id)
                    throw new RegraDeNegocioException("Uma categoria não pode ser mãe de si mesma.");

                await ValidarCategoriaPaiAsync(categoriaPaiId.Value);

                if ((await _categoriaRepository.ObterSubcategoriasAsync(id)).Any())
                    throw new RegraDeNegocioException("Uma categoria que tem subcategorias não pode virar subcategoria.");
            }

            categoria.Nome = nome.Trim();
            categoria.Slug = slug;
            categoria.CategoriaPaiId = categoriaPaiId;

            await _db.SaveChangesAsync();

            return categoria;
        }

        public async Task DesativarAsync(Guid id)
        {
            var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NaoEncontradoException("Categoria não encontrada.");

            var subcategorias = await _categoriaRepository.ObterSubcategoriasAsync(id);
            if (subcategorias.Any(s => s.Ativa))
                throw new ConflitoException("Desative as subcategorias antes de desativar esta categoria.");

            if (await _db.Produtos.AnyAsync(p => p.CategoriaId == id && p.Ativo))
                throw new ConflitoException("Desative os produtos desta categoria antes de desativá-la.");

            categoria.Ativa = false;
            await _db.SaveChangesAsync();
        }

        private async Task ValidarCategoriaPaiAsync(Guid categoriaPaiId)
        {
            var pai = await _categoriaRepository.ObterCategoriaPorIdAsync(categoriaPaiId)
                ?? throw new NaoEncontradoException("Categoria pai não encontrada.");

            if (pai.CategoriaPaiId is not null)
                throw new RegraDeNegocioException("Máximo de 2 níveis de categoria.");

            if (!pai.Ativa)
                throw new RegraDeNegocioException("A categoria pai está desativada.");
        }

        private static string GerarSlug(string texto)
        {
            var normalizado = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var semAcento = new string(normalizado
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());
            return Regex.Replace(semAcento, "[^a-z0-9]+", "-").Trim('-');
        }
    }
}
