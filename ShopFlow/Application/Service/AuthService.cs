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
        private readonly IGoogleTokenValidator _googleValidator;
        private readonly ShopFlowDbContext _db;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            IPasswordHasher passwordHasher,
            IGoogleTokenValidator googleValidator,
            ShopFlowDbContext db)
        {
            _usuarioRepository = usuarioRepository;
            _passwordHasher = passwordHasher;
            _googleValidator = googleValidator;
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

        public async Task<Usuario> LoginComGoogleAsync(string idToken)
        {
            // 1. VALIDAR o token com o Google (assinatura, validade e audience)
            var google = await _googleValidator.ValidarAsync(idToken);

            // Sem e-mail verificado pelo Google, não dá para confiar nele (risco de tomar conta alheia)
            if (!google.EmailVerificado)
                throw new NaoAutorizadoException("O e-mail da conta Google não está verificado.");

            var email = NormalizarEmail(google.Email);

            // 2. Já entrou antes pelo Google → é ele
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.GoogleId == google.GoogleId);
            if (usuario is not null)
                return usuario;

            // 3. Já existe conta com esse e-mail (cadastro por senha) → vincula o Google a ela
            usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario is not null)
            {
                usuario.GoogleId = google.GoogleId;
                usuario.EmailConfirmado = true;
                await _db.SaveChangesAsync();
                return usuario;
            }

            // 4. Primeira vez: cria a conta, sem senha
            usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = google.Nome.Trim(),
                Email = email,
                GoogleId = google.GoogleId,
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
