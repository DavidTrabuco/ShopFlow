# ---------- Estágio 1: build (a "cozinha") ----------
# Imagem com o SDK do .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primeiro só o .csproj, e restaura os pacotes (aproveita o cache)
COPY ShopFlow/ShopFlow.csproj ShopFlow/
RUN dotnet restore ShopFlow/ShopFlow.csproj

# Agora o resto do código, e compila em Release
COPY . .
RUN dotnet publish ShopFlow/ShopFlow.csproj -c Release -o /app/publish

# ---------- Estágio 2: runtime (a "embalagem") ----------
# Imagem só com o runtime (bem menor)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Pega SÓ o resultado do estágio 1
COPY --from=build /app/publish .

# A API escuta na porta 8080 dentro do container
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ShopFlow.dll"]
