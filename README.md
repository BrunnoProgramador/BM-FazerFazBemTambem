# FazerBemFazBemTambém

Sistema web de gestão de um projeto social: cadastro de alunos, turmas,
frequência (encontros), eventos, voluntários e doações.

**Stack:** ASP.NET Core MVC (.NET 10) · PostgreSQL (Neon) · Npgsql · ADO.NET puro (sem ORM)

## Estrutura

```
ProjetoAtual/
├── EM.Domain/       Entidades, fachadas e validadores (sem dependência de infra)
├── EM.Repository/   Acesso a dados (Postgres via Npgsql)
└── EM.Web/          Controllers, views e configuração
```

O schema do banco **não** é criado automaticamente na subida da aplicação
(diferente da versão antiga com Firebird). Ele precisa ser rodado uma
única vez, manualmente, via `schema_postgres.sql` — veja "Como rodar"
abaixo. Isso é intencional: em produção o schema é criado uma vez no
Neon; não há auto-migração em tempo de execução.

## Como rodar

### Dev container (recomendado)

Pré-requisitos: Docker e VS Code com a extensão Dev Containers.

Abra a pasta e use **Reopen in Container**. O compose sobe um Postgres
local (`db`, porta `5432`) e injeta a connection string por variável de
ambiente automaticamente.

Na primeira vez, rode o schema dentro do terminal do devcontainer:

```bash
psql -h db -U postgres -d projetoem -f schema_postgres.sql
```

(senha: `postgres` — só vale para o ambiente local)

Depois disso:

```bash
dotnet run --project ProjetoAtual/EM.Web
```

### Testar contra o Neon (staging/produção)

Sobrescreva a variável de ambiente antes de rodar, sem mexer no
`docker-compose.yml`:

```bash
export ConnectionStrings__Postgres="Host=SEU-HOST.neon.tech;Port=5432;Database=neondb;Username=USUARIO;Password=SENHA;SSL Mode=Require;Trust Server Certificate=true"
dotnet run --project ProjetoAtual/EM.Web
```

## Banco de dados (Neon)

- O schema completo está em `schema_postgres.sql` — rode uma única vez
  no editor SQL do Neon ao criar o projeto.
- Backup/cópias de segurança: o Neon oferece *branching* (cópia
  instantânea do banco) e *point-in-time restore* no dashboard — não
  depende de scripts locais como a versão antiga com Firebird.

## Deploy em produção

- **Aplicação:** Vercel, deploy automático a cada push na branch principal.
- **Banco:** Neon (Postgres serverless).
- Variável de ambiente necessária na Vercel: `ConnectionStrings__Postgres`
  (connection string do Neon, formato acima).

A pasta `deploy/` contém um caminho alternativo (self-hosted, VM +
Docker + Caddy + Firebird) de uma fase anterior do projeto — não é o
caminho usado atualmente, mantido só como referência.

## Testes

O projeto `EM.Testes` (xUnit) cobre validadores, hash de senha,
enumeradores, regras de domínio e fachadas (com repositórios fake em
memória):

```bash
dotnet test ProjetoAtual/ProjetoAtual.slnx
```

> ⚠️ **Pendência conhecida:** os testes de integração em
> `EM.Testes/Integracao/` (`BancoTestes.cs`, `FatoIntegracao.cs`) e o
> workflow `.github/workflows/ci.yml` ainda referenciam Firebird
> (variável `EM_TESTE_FIREBIRD`, serviço Firebird no CI). Isso precisa
> ser adaptado para Postgres antes de confiar nesses testes de
> integração ou reativar o CI.

## CI

`.github/workflows/ci.yml` compila a solução e roda os testes a cada
push/PR. **Ainda não foi adaptado para Postgres** (ver pendência acima)
— hoje ele sobe um Firebird de serviço que não é mais usado pelo
código da aplicação.
