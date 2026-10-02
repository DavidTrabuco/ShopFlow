using Microsoft.EntityFrameworkCore;
using ShopFlow.Domain.Entidades;
using ShopFlow.Domain.Enums;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Domain.Interfaces;
using ShopFlow.Infrastructure.Data;

namespace ShopFlow.Application.Service
{
    public class AuthService : IAuthService
    {


        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ShopFlowDbContext _db;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            IPasswordHasher passwordHasher,
            ShopFlowDbContext db)
        {
            _usuarioRepository = usuarioRepository;
            _passwordHasher = passwordHasher;
            _db = db;
        }

        public async Task<Usuario> RegistrarAsync(string nome, string email, string senha)
        {
            email = NormalizarEmail(email);

            if (await _usuarioRepository.EmailExisteAsync(email))
                throw new ConflitoException("Email já está em uso.");

            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = nome.Trim(),
                Email = email,
                SenhaHash = _passwordHasher.Gerar(senha),
                Papel = PapelUsuario.Cliente,
                EmailConfirmado = false,
                CriadoEm = DateTime.UtcNow
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();

            return usuario;
        }

        public async Task<Usuario> LoginAsync(string email, string senha)
        {
            email = NormalizarEmail(email);

            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);

            if (usuario is null || !_passwordHasher.Verificar(senha, usuario.SenhaHash))
                throw new NaoAutorizadoException("E-mail ou senha inválidos.");

            return usuario;
        }

        private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();




        // Sessão 
        public async Task CriarSessaoAsync(Guid usuarioId, string tokenHash)
        {
            _db.Sessoes.Add(new Sessao
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                TokenHash = tokenHash,
                CriadoEm = DateTime.UtcNow,
                ExpiraEm = DateTime.UtcNow.AddDays(7)
            });
            await _db.SaveChangesAsync();
        }

        public async Task<Usuario?> ValidarSessaoAsync(string tokenHash)
        {
            var sessao = await _db.Sessoes
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.TokenHash == tokenHash);

            if (sessao is null || !sessao.Ativa)
                return null;

            return sessao.Usuario;
        }

        public async Task EncerrarSessaoAsync(string tokenHash)
        {
            var sessao = await _db.Sessoes.FirstOrDefaultAsync(s => s.TokenHash == tokenHash);
            if (sessao is null) return;

            sessao.EncerradaEm = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }



        public async Task DeletarContaAsync(Guid usuarioId)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
            if (usuario is null)
                throw new NaoEncontradoException("Usuário não encontrado.");
            _db.Usuarios.Remove(usuario);
            await _db.SaveChangesAsync();
        }
    }
}
