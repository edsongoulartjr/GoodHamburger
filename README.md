# 🍔 Good Hamburger — Sistema de Pedidos

API REST + Frontend Blazor WASM para gerenciamento de pedidos de lanchonete.

---

## 🚀 Como executar

### Pré-requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### 1. Clonar e restaurar

```bash
git clone <repo-url>
cd GoodHamburger
dotnet restore
```

### 2. Executar a API

```bash
dotnet run --project src/GoodHamburger.API --launch-profile http
```

A API sobe em `http://localhost:5263`. O Swagger estará disponível em `http://localhost:5263/swagger`.

> O banco SQLite (`goodhamburger.db`) e as migrations são aplicados automaticamente na primeira execução.

### 3. Executar o Frontend (Blazor WASM)

Em outro terminal:

```bash
dotnet run --project src/GoodHamburger.Web --launch-profile http
```

Acesse `http://localhost:5253`.

### 4. Executar os testes

```bash
dotnet test tests/GoodHamburger.Tests/GoodHamburger.Tests.csproj
```

---

## ⚙️ Configuração

O JWT secret **não possui valor padrão** — deve ser definido por ambiente.

**Desenvolvimento** (`appsettings.Development.json` — já incluso no repositório):
```json
{
  "Jwt": { "Secret": "GoodHamburger-SuperSecret-Key-Min32Chars!!" },
  "Cors": { "AllowedOrigins": [ "http://localhost:5253" ] }
}
```

**Produção** — usar variáveis de ambiente ou Azure Key Vault:
```bash
export Jwt__Secret="<secret-forte-min-32-chars>"
export Cors__AllowedOrigins__0="https://meudominio.com"
```

---

## 📋 Endpoints da API

### 🔓 Públicos

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/menu` | Lista todos os itens do cardápio |
| POST | `/api/auth/register` | Registra novo usuário |
| POST | `/api/auth/login` | Autenticação (retorna JWT) |
| GET | `/health` | Health check |

> Os endpoints `/api/auth/*` possuem **rate limiting**: máximo de 10 requisições por minuto por IP.

### 🔐 Autenticados (Bearer Token)

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/orders?page=1&pageSize=10` | Lista pedidos paginados do usuário |
| GET | `/api/orders/{id}` | Busca pedido por ID |
| POST | `/api/orders` | Cria um novo pedido |
| PUT | `/api/orders/{id}` | Atualiza um pedido |
| DELETE | `/api/orders/{id}` | Remove um pedido |

### Exemplo — Criar pedido

```json
POST /api/orders
Authorization: Bearer <token>

{
  "menuItemIds": [
    "a1b2c3d4-0001-0001-0001-000000000001",
    "a1b2c3d4-0001-0001-0001-000000000004",
    "a1b2c3d4-0001-0001-0001-000000000005"
  ]
}
```

### Resposta paginada (`GET /api/orders`)

```json
{
  "items": [...],
  "page": 1,
  "pageSize": 10,
  "totalCount": 42,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

## 🏗️ Arquitetura

Clean Architecture com 5 camadas:

```
GoodHamburger/
├── src/
│   ├── GoodHamburger.Domain          # Entidades, Enums, Interfaces, Exceções, Result<T>
│   ├── GoodHamburger.Application     # Serviços, DTOs, Validators (FluentValidation)
│   ├── GoodHamburger.Infrastructure  # EF Core + SQLite, Repositórios, DI
│   ├── GoodHamburger.API             # Controllers, Middlewares, Program.cs, Swagger
│   └── GoodHamburger.Web             # Blazor WASM — Frontend
└── tests/
    └── GoodHamburger.Tests           # xUnit + FluentAssertions + NSubstitute
```

### Decisões técnicas

| Decisão | Motivo |
|---------|--------|
| **Clean Architecture** | Separação de responsabilidades, testabilidade e desacoplamento |
| **Interfaces para Services** | Controllers dependem de abstrações (DIP) — facilita testes e substituição |
| **SQLite** | Zero configuração, ideal para o contexto do desafio |
| **EF Core com Migrations** | Banco evolui junto com o código; seed automático do cardápio |
| **`MenuItemType` no `OrderItem`** | Desconto calculado sem navigation property — sem dependência de Include do EF |
| **Regras de desconto no Domain** | O desconto é lógica de negócio central — vive na entidade `Order` |
| **Record DTOs** | Imutabilidade garantida na camada de transporte |
| **FluentValidation** | Validação declarativa e testável separada dos serviços |
| **Result\<T\>** | Pattern para erros esperados sem uso de exceções no fluxo de controle |
| **Blazor WASM** | SPA client-side em C# puro |
| **xUnit + FluentAssertions** | Assertions legíveis e expressivas |
| **NSubstitute** | Mocking simples para testes unitários de serviço |
| **JWT Authentication** | Autenticação stateless, cada usuário vê apenas seus pedidos |
| **BCrypt** | Hash seguro de senhas |
| **Rate Limiting** | Proteção contra brute-force nos endpoints de autenticação |
| **Serilog** | Logs estruturados com rotação diária de arquivo e Correlation ID por requisição |
| **Global Exception Middleware** | Respostas padronizadas (ProblemDetails RFC 7807) |
| **Docker** | Dockerfile multi-stage + docker-compose para deploy simplificado |

### Regras de desconto implementadas

- Sanduíche + Batata + Refrigerante → **20%**
- Sanduíche + Refrigerante → **15%**
- Sanduíche + Batata → **10%**
- Apenas um item de cada categoria por pedido (validado com `DomainException`)
- Itens duplicados retornam erro claro

---

## ✅ Cobertura de testes (38 testes)

| Suite | Qtd | Escopo |
|-------|-----|--------|
| `OrderDiscountTests` | 7 | Desconto 20%, 15%, 10%, sem desconto, cálculo de total |
| `OrderServiceValidationTests` | 6 | Items vazios, duplicatas, dois sanduíches, duas bebidas, pedido válido, ID inexistente |
| `AuthServiceTests` | 4 | Register/Login — sucesso e falhas esperadas |
| `AuthValidatorTests` | 7 | Validação de e-mail, senha, nome via FluentValidation |
| `OrderValidatorTests` | 3 | Lista vazia, duplicatas, itens válidos |
| `AuthIntegrationTests` | 7 | Testes end-to-end com `WebApplicationFactory` + SQLite in-memory |

---

## 🐳 Docker

```bash
docker-compose up --build
```

A API ficará disponível em `http://localhost:5000`.

> Para produção, defina a variável `Jwt__Secret` no ambiente antes de subir o container.

---

## 🔑 Fluxo de Autenticação

1. **Registrar** — `POST /api/auth/register` com `{ name, email, password }`
2. **Login** — `POST /api/auth/login` com `{ email, password }` → retorna JWT
3. **Usar token** — Enviar `Authorization: Bearer <token>` nas rotas protegidas
4. Cada usuário vê e gerencia apenas **seus próprios pedidos**

---

## 📦 Pacotes principais

| Pacote | Uso |
|--------|-----|
| `Microsoft.EntityFrameworkCore.Sqlite` | ORM + banco de dados |
| `FluentValidation.AspNetCore` | Validação declarativa de requests |
| `Serilog.AspNetCore` | Logging estruturado |
| `BCrypt.Net-Next` | Hash seguro de senhas |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Autenticação JWT |
| `Microsoft.AspNetCore.RateLimiting` | Rate limiting nativo do ASP.NET Core |
| `xUnit` + `FluentAssertions` + `NSubstitute` | Testes unitários |
| `Microsoft.AspNetCore.Mvc.Testing` | Testes de integração |
