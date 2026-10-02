using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using ShopFlow.Domain.Entidades;
using ShopFlow.Domain.Interfaces;
using ShopFlow.Infrastructure.Data;

namespace ShopFlow.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ShopFlowDbContext _db;
        private IDbConnection Connection => _db.Database.GetDbConnection();

        public UsuarioRepository(ShopFlowDbContext db)
        {
            _db = db;
        }

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            const string query = """
                SELECT id, nome, email, senha_hash, cpf, telefone, papel, email_confirmado, criado_em
                FROM usuarios
                WHERE email = @Email
                """;
            return await Connection.QueryFirstOrDefaultAsync<Usuario>(query, new { Email = email });
        }

        public async Task<bool> EmailExisteAsync(string email)
        {
            const string query = "SELECT EXISTS(SELECT 1 FROM usuarios WHERE email = @Email)";
            return await Connection.ExecuteScalarAsync<bool>(query, new { Email = email });
        }



        public async Task<Usuario?> ObterPorIdAsync(Guid id)
        {
            const string query = """
                SELECT id, nome, email, senha_hash, cpf, telefone, papel, email_confirmado, criado_em
                FROM usuarios
                WHERE id = @Id
                """;
            return await Connection.QueryFirstOrDefaultAsync<Usuario>(query, new { Id = id });
        }
    }
}
