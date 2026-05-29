# ZDZ Catálogo — Gestão de Produtos e Categorias

## Pré-requisitos

| Ferramenta | Versão mínima |
|---|---|
| .NET SDK | 8.0 |
| SQL Server / SQL Server Express | 2019+ |
| Node.js | 18.x |
| npm | 9.x |
| EF Core CLI (`dotnet-ef`) | 8.x |

Instalação do EF Core CLI (caso não tenha):
```bash
dotnet tool install --global dotnet-ef
```

---

## Estrutura do Projeto

```
zdz-catalogo/
├── backend/
│   └── CatalogoApi/          # Web API .NET 8
└── frontend/
    └── catalogo-app/         # Nuxt 4 + Vuetify 4
```

---

## Backend — Restaurar pacotes NuGet e aplicar Migrations

### 1. Restaurar pacotes NuGet

```bash
cd backend/CatalogoApi
dotnet restore
```

### 2. Configurar a string de conexão

Abra `backend/CatalogoApi/appsettings.json` e ajuste conforme a sua instância do SQL Server:

```json
{
  "ConnectionStrings": {
    "ConexaoPadrao": "Server=.\\SQLEXPRESS;Database=CatalogoDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> Se estiver usando SQL Server padrão (não Express), substitua `.\\SQLEXPRESS` por `.` ou pelo nome da sua instância.

### 3. Aplicar as Migrations

```bash
cd backend/CatalogoApi
dotnet ef database update
```

O banco `CatalogoDB` será criado automaticamente com as tabelas `Categorias` e `Produtos`.

### 4. Rodar o backend

```bash
dotnet run
```

A API sobe em:
- HTTP: `http://localhost:5193`
- HTTPS: `https://localhost:7286`
- Swagger: `http://localhost:5193/swagger` ou `https://localhost:7286/swagger`

---

## Frontend — Restaurar pacotes Nuxt e rodar

### 1. Restaurar pacotes Node

```bash
cd frontend/catalogo-app
npm install
```

### 2. Rodar o frontend

```bash
npm run dev
```

A aplicação fica disponível em `http://localhost:3001`.

---

## Payloads esperados para teste

### Categorias — `POST /api/categorias`

```json
{
  "nome": "Móveis"
}
```

```json
{
  "nome": "Eletrônicos"
}
```

> Regra: `nome` deve ter no mínimo 5 caracteres.

---

### Categorias — `PUT /api/categorias/{id}`

```json
{
  "nome": "Móveis de Escritório"
}
```

---

### Produtos — `POST /api/produtos`

```json
{
  "nome": "Cadeira Ergonômica",
  "descricao": "Cadeira com apoio lombar",
  "preco": 850.00,
  "categoriaId": 1
}
```

```json
{
  "nome": "Monitor 27 polegadas",
  "descricao": "Monitor Full HD 144Hz",
  "preco": 1299.90,
  "categoriaId": 2
}
```

> Regras: `nome` mínimo 5 caracteres; `preco` maior que zero; `categoriaId` deve referenciar uma categoria existente.

---

### Produtos — `PUT /api/produtos/{id}`

```json
{
  "nome": "Cadeira Ergonômica Premium",
  "descricao": "Cadeira com apoio lombar e braço ajustável",
  "preco": 1100.00,
  "categoriaId": 1
}
```

---

### Deleção com integridade referencial

Tentar excluir uma categoria que possui produtos vinculados retorna:

```
HTTP 409 Conflict
```

```json
{
  "mensagem": "Não é possível excluir uma categoria que possua produtos vinculados."
}
```

---

## Ordem recomendada de execução

1. Iniciar o **backend** (`dotnet run`)
2. Iniciar o **frontend** (`npm run dev`)
3. Acessar `http://localhost:3001`
4. Criar categorias antes de criar produtos

---

## Relatório de Testes Automatizados

### Visão Geral

> Projeto: `backend/CatalogoApi.Tests`
> Framework: xUnit 2.9 + WebApplicationFactory + SQLite (banco relacional de teste)
> Última execução: 29/05/2026
> Tempo total de execução: **2,18 segundos**

| Resultado | Quantidade |
|---|---|
| Aprovados | **39** |
| Reprovados | 0 |
| Total | **39** |

---

### AC 02 — Modelagem Relacional (6 testes)

| # | Teste | Resultado |
|---|---|---|
| 1 | Categoria deve ter Id, Nome e Descricao | ✅ Aprovado |
| 2 | Produto deve ter Id, Nome, Descricao, Preco e CategoriaId | ✅ Aprovado |
| 3 | Produto deve ter propriedade de navegacao para Categoria (FK 1:N) | ✅ Aprovado |
| 4 | Categoria deve ter colecao de Produtos (lado N do 1:N) | ✅ Aprovado |
| 5 | CategoriaId em Produto deve ser do tipo int (chave estrangeira) | ✅ Aprovado |
| 6 | Preco em Produto deve ser do tipo decimal | ✅ Aprovado |

---

### AC 03 — Contrato de Endpoints (17 testes)

| # | Teste | Resultado |
|---|---|---|
| 7 | GET /api/categorias deve retornar 200 OK | ✅ Aprovado |
| 8 | GET /api/categorias deve retornar array JSON | ✅ Aprovado |
| 9 | POST /api/categorias com dados validos deve retornar 201 Created | ✅ Aprovado |
| 10 | POST /api/categorias deve retornar entidade criada com Id | ✅ Aprovado |
| 11 | PUT /api/categorias/{id} com dados validos deve retornar 204 NoContent | ✅ Aprovado |
| 12 | DELETE /api/categorias/{id} sem produtos deve retornar 204 NoContent | ✅ Aprovado |
| 13 | GET /api/produtos deve retornar 200 OK | ✅ Aprovado |
| 14 | GET /api/produtos deve retornar array JSON | ✅ Aprovado |
| 15 | GET /api/produtos deve incluir objeto categoria aninhado via .Include() | ✅ Aprovado |
| 16 | GET /api/produtos — categoria aninhada deve corresponder a categoria criada | ✅ Aprovado |
| 17 | POST /api/produtos com dados validos deve retornar 201 Created | ✅ Aprovado |
| 18 | POST /api/produtos deve retornar entidade criada com Id | ✅ Aprovado |
| 19 | PUT /api/produtos/{id} com dados validos deve retornar 204 NoContent | ✅ Aprovado |
| 20 | DELETE /api/produtos/{id} deve retornar 204 NoContent | ✅ Aprovado |
| 21 | DELETE /api/produtos/{id} deve remover produto do banco | ✅ Aprovado |

---

### AC 04 — Regra de Integridade Referencial (3 testes)

| # | Teste | Resultado |
|---|---|---|
| 22 | DELETE /api/categorias/{id} com produtos vinculados deve retornar 409 Conflict | ✅ Aprovado |
| 23 | Mensagem de erro deve ser exatamente "Não é possível excluir uma categoria que possua produtos vinculados." | ✅ Aprovado |
| 24 | Categoria com produtos vinculados deve permanecer no banco após tentativa de exclusão | ✅ Aprovado |

---

### AC 05 — Validação de Payload (9 testes)

| # | Teste | Resultado |
|---|---|---|
| 25 | POST /api/categorias com Nome nulo deve retornar 400 Bad Request | ✅ Aprovado |
| 26 | POST /api/categorias com Nome de 4 caracteres deve retornar 400 Bad Request | ✅ Aprovado |
| 27 | POST /api/categorias com Nome de exatamente 5 caracteres deve retornar 201 | ✅ Aprovado |
| 28 | PUT /api/categorias/{id} com Nome de 4 caracteres deve retornar 400 Bad Request | ✅ Aprovado |
| 29 | PUT /api/categorias/{id} com Nome nulo deve retornar 400 Bad Request | ✅ Aprovado |
| 30 | POST /api/produtos com Nome nulo deve retornar 400 Bad Request | ✅ Aprovado |
| 31 | POST /api/produtos com Nome de 4 caracteres deve retornar 400 Bad Request | ✅ Aprovado |
| 32 | POST /api/produtos com Nome de exatamente 5 caracteres deve retornar 201 | ✅ Aprovado |
| 33 | PUT /api/produtos/{id} com Nome de 4 caracteres deve retornar 400 Bad Request | ✅ Aprovado |
| 34 | PUT /api/produtos/{id} com Nome nulo deve retornar 400 Bad Request | ✅ Aprovado |

---

### AC 06 — Segurança de Comunicação — CORS (5 testes)

| # | Teste | Resultado |
|---|---|---|
| 35 | Origem http://localhost:3000 deve receber Access-Control-Allow-Origin correto | ✅ Aprovado |
| 36 | Origem http://localhost:3001 deve receber Access-Control-Allow-Origin correto | ✅ Aprovado |
| 37 | CORS não deve usar AllowAnyOrigin (*) — proibido pela especificação | ✅ Aprovado |
| 38 | Origem não listada não deve receber acesso liberado | ✅ Aprovado |
| 39 | Requisição de origem permitida deve retornar 200 OK | ✅ Aprovado |

---

### Como executar os testes

> O backend **não** precisa estar rodando. O `WebApplicationFactory` sobe a API internamente usando SQLite.

```bash


# Parar o backend (se estiver rodando) antes de executar
cd backend/CatalogoApi.Tests
dotnet test --verbosity normal



Swagger :
<img width="1816" height="967" alt="image" src="https://github.com/user-attachments/assets/0d1aff28-8bdf-4dd2-be61-db3ee494ff26" />

Front: <img width="1902" height="1020" alt="image" src="https://github.com/user-attachments/assets/ab74280a-3826-4724-9eb4-13e6296c1779" />

Banco de dados: <img width="786" height="681" alt="image" src="https://github.com/user-attachments/assets/b0d6e019-9928-4d92-bcb2-769aba4dc0ca" />




```
