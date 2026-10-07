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

            // Conta criada só pelo Google não tem senha: não dá para entrar por aqui
            if (usuario is null || usuario.SenhaHash is null || !_passwordHasher.Verificar(senha, usuario.SenhaHash))
                throw new NaoAutorizadoException("E-mail ou senha inválidos.");

            return usuario;
        }

        // Chamado pelo callback do Google: os dados já foram validados pelo middleware (troca do code)
        public async Task<Usuario> ObterOuCriarViaGoogleAsync(string googleId, string email, string nome, bool emailVerificado)
        {
            // Sem e-mail verificado pelo Google, não dá para confiar nele (risco de tomar conta alheia)
            if (!emailVerificado)
                throw new NaoAutorizadoException("O e-mail da conta Google não está verificado.");

            email = NormalizarEmail(email);

            // 1. Já entrou antes pelo Google → é ele
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.GoogleId == googleId);
            if (usuario is not null)
                return usuario;

            // 2. Já existe conta com esse e-mail (cadastro por senha) → vincula o Google a ela
            usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario is not null)
            {
                usuario.GoogleId = googleId;
                usuario.EmailConfirmado = true;
                await _db.SaveChangesAsync();
                return usuario;
            }

            // 3. Primeira vez: cria a conta, sem senha
            usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = nome.Trim(),
                Email = email,
                GoogleId = googleId,
                SenhaHash = null,   // conta só do Google: sem senha
                Papel = PapelUsuario.Cliente,
                EmailConfirmado = true,
                CriadoEm = DateTime.UtcNow
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();

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
            // Escrita → carrega pelo EF (entidade rastreada), não pelo Dapper
            var usuario = await _db.Usuarios.FindAsync(usuarioId)
                ?? throw new NaoEncontradoException("Usuário não encontrado.");

            _db.Usuarios.Remove(usuario);
            await _db.SaveChangesAsync();
        }


        public async Task AlterarPapelUsuarioAsync(Guid usuarioId, PapelUsuario novoPapel)
        {
            var usuario = await _db.Usuarios.FindAsync(usuarioId)
                ?? throw new NaoEncontradoException("Usuário não encontrado.");

            usuario.Papel = novoPapel;
            await _db.SaveChangesAsync();
        }
    }
}
