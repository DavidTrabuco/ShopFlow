using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopFlow.API.Extensao;
using ShopFlow.Application.Service;
using ShopFlow.Domain.Enums;
using ShopFlow.Domain.Interfaces;
using ShopFlow.Domain.Interfaces.Categorias;
using ShopFlow.Domain.Interfaces.Produtos;
using ShopFlow.Domain.Interfaces.Variantes;
using ShopFlow.Domain.Options;
using ShopFlow.Infrastructure.Data;
using ShopFlow.Infrastructure.Repositories;
using ShopFlow.Infrastructure.Security;
using ShopFlow.Middlewares;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.Secao))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<GoogleAuthOptions>()
    .Bind(builder.Configuration.GetSection(GoogleAuthOptions.Secao))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDbContext<ShopFlowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ShopFlow"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();

// Erros padronizados: tudo sai como ProblemDetails (RFC 9457)
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSwaggerGen();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;



builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IVarianteRepository, VarianteRepository>();
builder.Services.AddScoped<IVarianteService, VarianteService>();



var jwt = builder.Configuration.GetSection(JwtOptions.Secao).Get<JwtOptions>()!;

var google = builder.Configuration.GetSection(GoogleAuthOptions.Secao).Get<GoogleAuthOptions>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),

        };


        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["access_token"];
                return Task.CompletedTask;
            }
        };
    })
    // Cookie temporário que carrega os dados do Google entre o redirect e o callback
    .AddCookie(EsquemasAuth.Externo, options => options.ExpireTimeSpan = TimeSpan.FromMinutes(5))
    .AddGoogle(options =>
    {
        options.ClientId = google.ClientId;
        options.ClientSecret = google.ClientSecret;
        options.SignInScheme = EsquemasAuth.Externo;
        // Para onde o Google devolve o usuário. O middleware trata sozinho (não é um endpoint do controller)
        // e precisa ser DIFERENTE da rota do controller, senão dá "oauth state was missing or invalid".
        options.CallbackPath = "/api/v1/auth/google/signin-callback";
        // O Google informa se o e-mail foi verificado; o AuthService exige isso
        options.ClaimActions.MapJsonKey("email_verified", "email_verified");
    });




builder.Services.AddAuthorization(option =>
{
    option.AddPolicy(Policies.Admin, policy => policy.RequireRole(PapelUsuario.Administrador.ToString()));

});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// PRIMEIRO do pipeline: lê o "bilhete" do proxy do Render (https + IP real)
app.UseForwardedHeaders();

// Cria/atualiza as tabelas ao subir (banco do Render nasce vazio)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopFlowDbContext>();
    db.Database.Migrate();
}

// Primeiro do pipeline: envolve tudo o que vem depois
app.UseExceptionHandler();
// 401/403/404 sem corpo (ex.: token inválido no JwtBearer) também viram ProblemDetails
app.UseStatusCodePages();

// Swagger também em produção (projeto de estudo/portfólio)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();


app.UseAuthentication();

app.UseAuthorization();


app.MapControllers();

app.Run();
