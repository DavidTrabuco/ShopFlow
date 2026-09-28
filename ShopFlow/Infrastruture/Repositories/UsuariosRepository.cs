using ShopFlow.Domain.Interfaces;
using ShopFlow.Infrastruture.Data;
using Dapper;
using System.Data;
using ShopFlow.Domain.Entidades;
using Microsoft.EntityFrameworkCore;


namespace ShopFlow.Infrastruture.Repositories
{
    public class UsuariosRepository : IUsuarioRepository
    {
        private readonly ShopFlowDbContext _db;
        private IDbConnection Connection => _db.Database.GetDbConnection();


        public UsuariosRepository(ShopFlowDbContext db)
        {
            _db = db;
        }

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            var query = "SELECT * FROM Usuarios WHERE Email = @Email";
            return await Connection.QueryFirstOrDefaultAsync<Usuario>(query, new { Email = email });
        }

        public async Task<bool> EmailExisteAsync(string email)
        {
            var query = "SELECT COUNT(1) FROM Usuarios WHERE Email = @Email";
            var count = await Connection.ExecuteScalarAsync<int>(query, new { Email = email });
            return count > 0;
        }




    }
}
