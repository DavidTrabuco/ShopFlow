using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using ShopFlow.Domain.Entidades;
using ShopFlow.Domain.Interfaces.Categorias;
using ShopFlow.Infrastructure.Data;

namespace ShopFlow.Infrastructure.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private const string Colunas = "id, nome, slug, ativa, categoria_pai_id";

        private readonly ShopFlowDbContext _db;
        private IDbConnection Connection => _db.Database.GetDbConnection();

        public CategoriaRepository(ShopFlowDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Categoria>> ObterCategoriasAsync()
        {
            var sql = $"SELECT {Colunas} FROM categorias ORDER BY nome";
            return await Connection.QueryAsync<Categoria>(sql);
        }

        public async Task<Categoria?> ObterCategoriaPorIdAsync(Guid id)
        {
            var sql = $"SELECT {Colunas} FROM categorias WHERE id = @Id";
            return await Connection.QueryFirstOrDefaultAsync<Categoria>(sql, new { Id = id });
        }

        public async Task<Categoria?> ObterCategoriaPorSlugAsync(string slug)
        {
            var sql = $"SELECT {Colunas} FROM categorias WHERE slug = @Slug";
            return await Connection.QueryFirstOrDefaultAsync<Categoria>(sql, new { Slug = slug });
        }

        public async Task<IEnumerable<Categoria>> ObterSubcategoriasAsync(Guid categoriaPaiId)
        {
            var sql = $"SELECT {Colunas} FROM categorias WHERE categoria_pai_id = @CategoriaPaiId";
            return await Connection.QueryAsync<Categoria>(sql, new { CategoriaPaiId = categoriaPaiId });
        }
    }
}
