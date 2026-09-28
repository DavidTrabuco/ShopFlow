using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopFlow.Application.Service;
using ShopFlow.Domain.Interfaces;
using ShopFlow.Domain.Options;
using ShopFlow.Infrastruture.Data;
using ShopFlow.Infrastruture.Repositories;
using ShopFlow.Infrastruture.Security;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.Secao))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDbContext<ShopFlowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ShopFlow"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;



builder.Services.AddScoped<IUsuarioRepository, UsuariosRepository>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();


var jwt = builder.Configuration.GetSection(JwtOptions.Secao).Get<JwtOptions>()!;

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
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
