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

**3. Configurar os segredos** (na pasta `ShopFlow`)

Três valores, guardados com `user-secrets` (ficam fora do repositório):

```bash
dotnet user-secrets set "Jwt:Key" "uma-chave-aleatoria-com-pelo-menos-32-caracteres"
dotnet user-secrets set "Google:ClientId" "SEU_ID.apps.googleusercontent.com"
dotnet user-secrets set "Google:ClientSecret" "GOCSPX-sua-chave-secreta"
```

| Chave | O que é | Obrigatório |
|---|---|---|
| `Jwt:Key` | Chave que assina os tokens de login. Cada máquina usa a sua. Sem ela a API não sobe (`OptionsValidationException`). | Sim |
| `Google:ClientId` | **ID do cliente** OAuth do Google (termina em `.apps.googleusercontent.com`). | Sim |
| `Google:ClientSecret` | **Chave secreta do cliente** OAuth (começa com `GOCSPX-`). Nunca vai para o repositório nem para o front. | Sim |

Os três são obrigatórios: sem algum deles a API não sobe e mostra qual está faltando (`OptionsValidationException`).

Cole os valores sem espaço ou quebra de linha, e não troque um pelo outro: o ID **não** começa com `GOCSPX-`.

**4. Rodar a API**
```bash
dotnet run --launch-profile https
```
Swagger: https://localhost:7202/swagger

Use sempre o perfil **https**: o cookie de autenticação é `Secure` e é descartado pelo navegador em `http`.

## Login com Google

Usa o **Authorization Code Flow** (com callback), feito inteiramente pela API. Para entrar, basta abrir no navegador:

```
https://localhost:7202/api/v1/auth/google
```

**Como funciona**
```
Navegador                Sua API                          Google
   |-- GET /auth/google ------>|                              |
   |<-- 302 para o Google -----|                              |
   |-------------- login e consentimento ------------------->|
   |<-- 302 para /auth/google/signin-callback?code=... ------|   (callback)
   |-- GET /auth/google/signin-callback -->|                   |
   |                           |-- troca o code (ClientSecret)->|
   |                           | grava o cookie temporário "External"
   |<-- 302 para /auth/google/callback ----|                   |
   |-- GET /auth/google/callback ->|                           |
   |                           | acha, vincula ou cria o usuário
   |<-- 200 + cookies de sessão + dados do usuário ----------|
```

- `GET /auth/google` (ida): manda o navegador para o Google.
- `/auth/google/signin-callback` (volta do Google): tratado sozinho pelo middleware, não existe no controller. É o que se cadastra no Google Cloud.
- `GET /auth/google/callback`: lê os dados do Google, acha ou cria o usuário (vincula pelo e-mail se já existir conta, exigindo e-mail verificado) e abre a sessão com os mesmos cookies do login normal.

**Obter o Client ID e o Client Secret** (uma vez)
1. Em [console.cloud.google.com](https://console.cloud.google.com), crie um projeto.
2. **APIs e serviços › Tela de consentimento OAuth**: preencha o app e, enquanto estiver em "Testing", adicione seu e-mail em **Usuários de teste**.
3. **Credenciais › Criar credenciais › ID do cliente OAuth**, tipo **Aplicativo da Web**.
4. Em **URIs de redirecionamento autorizados**, cadastre a URL de callback de cada ambiente, exatamente assim:
   ```
   https://localhost:7202/api/v1/auth/google/signin-callback
   https://SEU-APP.onrender.com/api/v1/auth/google/signin-callback
   ```
5. Copie o **ID do cliente** e a **chave secreta do cliente** para o passo 3 acima. Mudanças no Google podem levar de alguns minutos a algumas horas.

Em "Origens JavaScript autorizadas" não precisa cadastrar nada, porque não há botão do Google numa página.

## Deploy (Render)

Variáveis de ambiente do serviço (**Environment**). Use dois underscores no lugar dos `:`:

| Variável | Valor |
|---|---|
| `ConnectionStrings__ShopFlow` | connection string do PostgreSQL |
| `Jwt__Key` | chave aleatória com pelo menos 32 caracteres |
| `Google__ClientId` | ID do cliente OAuth do Google |
| `Google__ClientSecret` | chave secreta do cliente OAuth do Google |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

As migrations são aplicadas sozinhas quando a API sobe.

## Endpoints disponíveis
| Método | Rota | Autenticação |
|---|---|---|
| POST | `/api/v1/auth/registrar` | pública |
| POST | `/api/v1/auth/login` | pública |
| GET | `/api/v1/auth/google` | pública (redireciona para o login do Google) |
| GET | `/api/v1/auth/google/callback` | pública (só é chamada no retorno do Google) |
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


<img width="1289" height="1826" alt="image" src="https://github.com/user-attachments/assets/bc926471-a007-448d-ad8b-09dd4e9e2940" />


## Problemas comuns
| Erro | Causa | Solução |
|---|---|---|
| `OptionsValidationException` … `Key` | `Jwt:Key` não configurada | passo 3 |
| `Failed to connect to 127.0.0.1:5432` | Postgres não está rodando | `docker compose up -d` e `docker ps` |
| `container name "/postgres" is already in use` | outro projeto usando o mesmo nome de container | pare o outro container ou mude o `container_name` |
| login retorna 200, mas `/me` retorna 401 | API rodando em `http` | rode com o perfil `https` |
| `OptionsValidationException` … `Google:ClientId` / `Google:ClientSecret` | valor do Google não configurado (local ou no Render) | passo 3 (local) ou variáveis `Google__ClientId` e `Google__ClientSecret` (Render) |
| Google: `Erro 401: invalid_client` / "OAuth client was not found" | `Google:ClientId` errado: chave secreta (`GOCSPX-...`) no lugar do ID, ou espaço/quebra de linha colados | regrave o valor com o ID exato do Google Cloud |
| Google: `Erro 401: invalid_client` / "Unauthorized" depois de escolher a conta | `Google:ClientSecret` errado ou de outro cliente OAuth | regrave com a chave secreta do mesmo cliente do ID |
| Google: `redirect_uri_mismatch` | a URL de callback não está em "URIs de redirecionamento autorizados" | cadastre exatamente `https://<host>/api/v1/auth/google/signin-callback` e aguarde alguns minutos |
| `The oauth state was missing or invalid` | o `CallbackPath` é igual à rota do controller, ou o cookie de correlação se perdeu (API reiniciou no meio do login) | mantenha `/auth/google/signin-callback` diferente de `/auth/google/callback` e tente de novo do começo |
| callback retorna 401 "e-mail ... não está verificado" | a conta Google não tem e-mail verificado | use outra conta Google |
