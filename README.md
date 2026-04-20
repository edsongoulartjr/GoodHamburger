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
cd src/GoodHamburger.API
dotnet run
```

A API sobe em `http://localhost:5000`. O Swagger estará disponível em `http://localhost:5000/swagger`.

> O banco SQLite (`goodhamburger.db`) e as migrations são aplicados automaticamente na primeira execução.

### 3. Executar o Frontend (Blazor WASM)

Em outro terminal:

```bash
cd src/GoodHamburger.Web
dotnet run
```

Acesse `http://localhost:5010` (ou a porta exibida no terminal).

### 4. Executar os testes

```bash
dotnet test tests/GoodHamburger.Tests/GoodHamburger.Tests.csproj
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

### 🔐 Autenticados (Bearer Token)

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/orders` | Lista pedidos do usuário autenticado |
| GET | `/api/orders/{id}` | Busca pedido por ID (do usuário) |
| POST | `/api/orders` | Cria um novo pedido |
| PUT | `/api/orders/{id}` | Atualiza um pedido |
| DELETE | `/api/orders/{id}` | Remove um pedido |

### Exemplo — Criar pedido

```json
POST /api/orders
{
  "menuItemIds": [
    "a1b2c3d4-0001-0001-0001-000000000001",
    "a1b2c3d4-0001-0001-0001-000000000004",
    "a1b2c3d4-0001-0001-0001-000000000005"
  ]
}
```

---

## 🏗️ Arquitetura

Clean Architecture com 4 camadas:

```
GoodHamburger/
├── src/
│   ├── GoodHamburger.Domain          # Entidades, Enums, Interfaces, Exceções
│   ├── GoodHamburger.Application     # Serviços, DTOs — lógica de negócio pura
│   ├── GoodHamburger.Infrastructure  # EF Core + SQLite, Repositórios, DI
│   └── GoodHamburger.API             # Controllers, Program.cs, Swagger
│   └── GoodHamburger.Web             # Blazor WASM — Frontend
└── tests/
    └── GoodHamburger.Tests           # xUnit + FluentAssertions + NSubstitute
```

### Decisões técnicas

| Decisão | Motivo |
|---------|--------|
| **Clean Architecture** | Separação de responsabilidades, testabilidade e desacoplamento |
| **SQLite** | Zero configuração, ideal para o contexto do desafio |
| **EF Core com Migrations** | Banco evolui junto com o código; seed automático do cardápio |
| **Regras de desconto no Domain** | O desconto é lógica de negócio central — vive na entidade `Order` |
| **Record DTOs** | Imutabilidade garantida na camada de transporte |
| **Blazor WASM** | Requisito do desafio; SPA client-side em C# puro |
| **xUnit + FluentAssertions** | Assertions legíveis e expressivas |
| **NSubstitute** | Mocking simples para testes de serviço sem banco |
| **JWT Authentication** | Autenticação stateless, cada usuário vê apenas seus pedidos |
| **BCrypt** | Hash seguro de senhas |
| **Global Exception Middleware** | Respostas padronizadas (ProblemDetails RFC 7807) |
| **Docker** | Dockerfile multi-stage + docker-compose para deploy simplificado |

### Regras de desconto implementadas

- Sanduíche + Batata + Refrigerante → **20%**
- Sanduíche + Refrigerante → **15%**
- Sanduíche + Batata → **10%**
- Apenas um item de cada categoria por pedido (validado com `DomainException`)
- Itens duplicados retornam erro claro

---

## ✅ Cobertura de testes (14 testes)

| Suite | Testes |
|-------|--------|
| `OrderDiscountTests` | Desconto 20%, 15%, 10%, sem desconto (só sanduíche, só acompanhamento, só bebida), cálculo de total |
| `OrderServiceValidationTests` | Items vazios, duplicatas, dois sanduíches, duas bebidas, pedido válido, ID inexistente |

---

## 🐳 Docker

```bash
docker-compose up --build
```

A API ficará disponível em `http://localhost:5000`.

---

## 🔑 Fluxo de Autenticação

1. **Registrar** — `POST /api/auth/register` com `{ name, email, password }`
2. **Login** — `POST /api/auth/login` com `{ email, password }` → retorna JWT
3. **Usar token** — Enviar `Authorization: Bearer <token>` nas rotas protegidas
4. Cada usuário vê e gerencia apenas **seus próprios pedidos**

---

## ❌ O que ficou fora

- Paginação na listagem de pedidos
- Testes de integração e end-to-end
- CI/CD pipeline
- Validação via FluentValidation no request body
