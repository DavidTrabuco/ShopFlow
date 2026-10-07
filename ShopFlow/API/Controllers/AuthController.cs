using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShopFlow.API.Extensao;
using ShopFlow.Application.DTO.Request;
using ShopFlow.Application.DTO.Response;
using ShopFlow.Domain.Entidades;
using ShopFlow.Domain.Interfaces;
using ShopFlow.Domain.Options;

namespace ShopFlow.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private const string CookieAcesso = "access_token";
        private const string CookieSessao = "sessao";
        private const int DiasSessao = 7;

        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly JwtOptions _jwt;
        private readonly GoogleOptions _google;

        public AuthController(IAuthService authService, ITokenService tokenService, IOptions<JwtOptions> jwt, IOptions<GoogleOptions> google)
        {
            _authService = authService;
            _tokenService = tokenService;
            _jwt = jwt.Value;
            _google = google.Value;
        }

        // O Client ID não é segredo: o front precisa dele para mostrar o botão do Google
        [HttpGet("google/config")]
        public IActionResult ConfigGoogle()
        {
            return Ok(new { clientId = _google.ClientId });
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar(RegistrarRequest request)
        {
            // E-mail em uso → ConflitoException → 409 (tratado no GlobalExceptionHandler)
            var usuario = await _authService.RegistrarAsync(request.Nome, request.Email, request.Senha);
            await AbrirSessaoAsync(usuario);
            return Ok(AuthResponse.De(usuario));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            // Credenciais inválidas → NaoAutorizadoException → 401 (tratado no GlobalExceptionHandler)
            var usuario = await _authService.LoginAsync(request.Email, request.Senha);
            await AbrirSessaoAsync(usuario);
            return Ok(AuthResponse.De(usuario));
        }

        [HttpPost("google")]
        public async Task<IActionResult> LoginComGoogle(GoogleLoginRequest request)
        {
            // Token inválido → NaoAutorizadoException → 401 (tratado no GlobalExceptionHandler)
            var usuario = await _authService.LoginComGoogleAsync(request.IdToken);
            await AbrirSessaoAsync(usuario);
            return Ok(AuthResponse.De(usuario));
        }

        [HttpPost("renovar")]
        public async Task<IActionResult> Renovar()
        {
            if (!Request.Cookies.TryGetValue(CookieSessao, out var tokenSessao))
                return Problem(title: "Não autorizado", detail: "Sessão expirada.", statusCode: StatusCodes.Status401Unauthorized);

            var hash = _tokenService.HashTokenSessao(tokenSessao);
            var usuario = await _authService.ValidarSessaoAsync(hash);

            if (usuario is null)
                return Problem(title: "Não autorizado", detail: "Sessão expirada.", statusCode: StatusCodes.Status401Unauthorized);

            await _authService.EncerrarSessaoAsync(hash);
            await AbrirSessaoAsync(usuario);
            return Ok(AuthResponse.De(usuario));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue(CookieSessao, out var tokenSessao))
                await _authService.EncerrarSessaoAsync(_tokenService.HashTokenSessao(tokenSessao));

            Response.Cookies.Delete(CookieAcesso, OpcoesCookie(null));
            Response.Cookies.Delete(CookieSessao, OpcoesCookie(null));
            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Nome = User.FindFirstValue(ClaimTypes.Name),
                Email = User.FindFirstValue(ClaimTypes.Email),
                Papel = User.FindFirstValue(ClaimTypes.Role)
            });
        }


        [Authorize]
        [HttpDelete("deletar")]
        public async Task<IActionResult> DeletarContaAsync()
        {
            var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));



            await _authService.DeletarContaAsync(usuarioId);


            Response.Cookies.Delete(CookieAcesso, OpcoesCookie(null));
            Response.Cookies.Delete(CookieSessao, OpcoesCookie(null));
            return NoContent();
        }



        //[Authorize(Policy = Policies.Admin)]
        [HttpPatch("usuarios/{id:guid}/papel")]
        public async Task<IActionResult> AlterarPapel(Guid id, PapelRequest request)
        {
            await _authService.AlterarPapelUsuarioAsync(id, request.PapelUsuario);
            return NoContent();
        }


        private async Task AbrirSessaoAsync(Usuario usuario)
        {
            var acesso = _tokenService.GerarToken(usuario);
            var tokenSessao = _tokenService.GerarTokenSessao();

            await _authService.CriarSessaoAsync(usuario.Id, _tokenService.HashTokenSessao(tokenSessao));

            Response.Cookies.Append(CookieAcesso, acesso,
                OpcoesCookie(DateTimeOffset.UtcNow.AddMinutes(_jwt.ExpiracaoMinutos)));

            Response.Cookies.Append(CookieSessao, tokenSessao,
                OpcoesCookie(DateTimeOffset.UtcNow.AddDays(DiasSessao)));
        }

        private static CookieOptions OpcoesCookie(DateTimeOffset? expira) => new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = expira
        };
    }
}
