using Dapper;
using ShopFlow.Infrastructure.Data;
using System.Data;
using ShopFlow.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using ShopFlow.Domain.Interfaces.Produtos;

namespace ShopFlow.Infrastructure.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {

        private readonly ShopFlowDbContext _db;
        private IDbConnection Connection => _db.Database.GetDbConnection();

        public ProdutoRepository(ShopFlowDbContext db)
        {
            _db = db;
        }



        public async Task<IEnumerable<Produto>> ObterProdutosAsync()
        {
            const string query = "SELECT * FROM produtos";
            return await Connection.QueryAsync<Produto>(query);
        }

        public async Task<Produto?> ObterProdutoPorIdAsync(Guid id)
        {
            const string query = "SELECT * FROM produtos WHERE id = @Id";
            return await Connection.QueryFirstOrDefaultAsync<Produto>(query, new { Id = id });
        }


    }
}
