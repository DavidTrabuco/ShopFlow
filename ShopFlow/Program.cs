using Microsoft.EntityFrameworkCore;
using ShopFlow.Domain.Interfaces;
using ShopFlow.Domain.Options;
using ShopFlow.Infrastruture.Data;
using System.Text.Json.Serialization;
using ShopFlow.Infrastruture.Repositories;

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




var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
