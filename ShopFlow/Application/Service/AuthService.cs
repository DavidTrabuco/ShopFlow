using ShopFlow.Application.DTO.Request;
using ShopFlow.Application.DTO.Response;
using ShopFlow.Domain.Entidades;
using ShopFlow.Domain.Enums;
using ShopFlow.Domain.Exceptions;
using ShopFlow.Domain.Interfaces;
using ShopFlow.Infrastruture.Data;

namespace ShopFlow.Application.Service
{
    public class AuthService : IAuthService
    {
        

        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly ShopFlowDbContext _db;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            ShopFlowDbContext db)
        {
            _usuarioRepository = usuarioRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _db = db;
        }

        public async Task<AuthResponse> RegistrarAsync(RegistrarRequest request, CancellationToken ct = default)
        {
            var email = NormalizarEmail(request.Email);

            if (await _usuarioRepository.EmailExisteAsync(email))
                throw new ConflitoException("Email já está em uso.");

            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = request.Nome.Trim(),
                Email = email,
                SenhaHash = _passwordHasher.Gerar(request.Senha),
                Papel = PapelUsuario.Cliente,
                EmailConfirmado = false,
                CriadoEm = DateTime.UtcNow
            };

            _db.Usuarios.Add(usuario);

          
            return MontarResposta(usuario);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var email = NormalizarEmail(request.Email);

            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);

            if (usuario is null || !_passwordHasher.Verificar(request.Senha, usuario.SenhaHash))
                throw new NaoAutorizadoException("E-mail ou senha inválidos.");

            return MontarResposta(usuario);
        }

        private AuthResponse MontarResposta(Usuario usuario)
        {
            return new AuthResponse
            {
                Nome = usuario.Nome,
                Email = usuario.Email,
                PapelUsuario = usuario.Papel,
                Token = _tokenService.GerarToken(usuario)
            };
        }

        private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
    }
}
