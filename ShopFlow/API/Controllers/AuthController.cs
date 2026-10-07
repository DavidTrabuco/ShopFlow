using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShopFlow.API.Extensao;
using ShopFlow.Application.DTO.Request;
using ShopFlow.Application.DTO.Response;
using ShopFlow.Domain.Entidades;
using ShopFlow.Domain.Exceptions;
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

        public AuthController(IAuthService authService, ITokenService tokenService, IOptions<JwtOptions> jwt)
        {
            _authService = authService;
            _tokenService = tokenService;
            _jwt = jwt.Value;
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar(RegistrarRequest request)
        {
            var usuario = await _authService.RegistrarAsync(request.Nome, request.Email, request.Senha);
            await AbrirSessaoAsync(usuario);
            return Ok(AuthResponse.De(usuario));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            
            var usuario = await _authService.LoginAsync(request.Email, request.Senha);
            await AbrirSessaoAsync(usuario);
            return Ok(AuthResponse.De(usuario));
        }

        // IDA: manda o navegador para a tela de login do Google
        [HttpGet("google")]
        public IActionResult LoginGoogle()
        {
            // O Challenge redireciona para o Google; RedirectUri é para onde voltar DEPOIS do middleware
            return Challenge(
                new AuthenticationProperties { RedirectUri = Url.Action(nameof(GoogleCallback)) },
                GoogleDefaults.AuthenticationScheme);
        }

        // VOLTA: o Google devolveu o usuário; o middleware já trocou o code e gravou o cookie "External".
        // Só é alcançado por redirecionamento, nunca chamado à mão: por isso fica fora do Swagger.
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var resultado = await HttpContext.AuthenticateAsync(EsquemasAuth.Externo);
            if (!resultado.Succeeded || resultado.Principal is null)
                throw new NaoAutorizadoException("Não foi possível entrar com o Google. Tente novamente.");

            var googleId = resultado.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = resultado.Principal.FindFirstValue(ClaimTypes.Email);
            var nome = resultado.Principal.FindFirstValue(ClaimTypes.Name) ?? email;
            var emailVerificado = string.Equals(
                resultado.Principal.FindFirstValue("email_verified"), "true", StringComparison.OrdinalIgnoreCase);

            // O cookie "External" era só um carrinho de mão: descarta
            await HttpContext.SignOutAsync(EsquemasAuth.Externo);

            if (googleId is null || email is null || nome is null)
                throw new NaoAutorizadoException("O Google não devolveu os dados da conta.");

            
            var usuario = await _authService.ObterOuCriarViaGoogleAsync(googleId, email, nome, emailVerificado);
            await AbrirSessaoAsync(usuario);   // o mesmo método do login normal
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
