using Dapper;
using ShopFlow.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using ShopFlow.Domain.Interfaces.Variantes;
using ShopFlow.Infrastructure.Data;
using System.Data;

namespace ShopFlow.Infrastructure.Repositories
{
    public class VarianteRepository : IVarianteRepository
    {

        private readonly ShopFlowDbContext _db;

        private IDbConnection Connection => _db.Database.GetDbConnection();
        public VarianteRepository(ShopFlowDbContext db)
        {
            _db = db;

        }


        public async Task<IEnumerable<Variante>> ObterVariantesAsync()
        {
            const string query = "SELECT * FROM variantes";
            var variantes = (await Connection.QueryAsync<Variante>(query)).ToList();
            await CarregarAtributosAsync(variantes);
            return variantes;
        }

        public async Task<Variante?> ObterPorIdAsync(Guid id)
        {
            const string query = "SELECT * FROM variantes WHERE id = @Id";
            var variante = await Connection.QueryFirstOrDefaultAsync<Variante>(query, new { Id = id });
            if (variante is not null)
                await CarregarAtributosAsync(new[] { variante });
            return variante;
        }

        // O Dapper não monta a lista sozinho: buscamos os atributos e entregamos a cada variante
        private async Task CarregarAtributosAsync(IReadOnlyCollection<Variante> variantes)
        {
            if (variantes.Count == 0)
                return;

            const string query = "SELECT * FROM atributos_variante WHERE variante_id = ANY(@Ids)";
            var atributos = await Connection.QueryAsync<AtributoVariante>(query, new { Ids = variantes.Select(v => v.Id).ToArray() });

            foreach (var variante in variantes)
                variante.Atributos = atributos.Where(a => a.VarianteId == variante.Id).ToList();
        }
    }
}
