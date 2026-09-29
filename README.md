# ShopFlow

API de e-commerce B2C em ASP.NET Core (.NET 10), com foco em confiabilidade de estoque e pagamento.

## Stack
.NET 10 · Minimal API · EF Core 10 + Npgsql · Dapper · PostgreSQL 15 (Docker) · JWT em cookie HttpOnly · BCrypt · Swagger

## Pré-requisitos
- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Ferramenta de migrations do EF Core:
  ```bash
  dotnet tool update --global dotnet-ef
  ```

## Como rodar

**1. Subir o PostgreSQL** (na raiz do repositório)
```bash
docker compose up -d
```
Cria um Postgres vazio na porta `5432` (usuário, senha e banco: `shopflow`). Os dados ficam na pasta `data/`, que não é versionada.

**2. Criar as tabelas** (na pasta `ShopFlow`)
```bash
cd ShopFlow
dotnet ef database update
```

**3. Configurar a chave do JWT** (na pasta `ShopFlow`)
```bash
dotnet user-secrets set "Jwt:Key" "uma-chave-aleatoria-com-pelo-menos-32-caracteres"
```
Cada máquina usa a sua própria chave. Sem ela, a API não sobe (`OptionsValidationException`).

**4. Rodar a API**
```bash
dotnet run --launch-profile https
```
Swagger: https://localhost:7202/swagger

Use sempre o perfil **https**: o cookie de autenticação é `Secure` e é descartado pelo navegador em `http`.

## Endpoints disponíveis
| Método | Rota | Autenticação |
|---|---|---|
| POST | `/api/v1/auth/registrar` | pública |
| POST | `/api/v1/auth/login` | pública |
| POST | `/api/v1/auth/logout` | pública |
| GET | `/api/v1/auth/me` | cookie `access_token` |

## Estrutura
```
ShopFlow/
├── Application/     DTOs e services (regra de negócio)
├── Domain/          entidades, enums, interfaces, options, exceptions
├── Endpoints/       Minimal API
├── Infrastructure/  DbContext, repositories (Dapper), segurança (JWT, BCrypt)
└── Migrations/      estrutura do banco (EF Core)
```

## Problemas comuns
| Erro | Causa | Solução |
|---|---|---|
| `OptionsValidationException` … `Key` | `Jwt:Key` não configurada | passo 3 |
| `Failed to connect to 127.0.0.1:5432` | Postgres não está rodando | `docker compose up -d` e `docker ps` |
| `container name "/postgres" is already in use` | outro projeto usando o mesmo nome de container | pare o outro container ou mude o `container_name` |
| login retorna 200, mas `/me` retorna 401 | API rodando em `http` | rode com o perfil `https` |
