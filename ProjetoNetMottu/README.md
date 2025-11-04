# NeoMoto API

## **Membros**

- Afonso Correia Pereira - RM557863
- Adel Mouhaidly - RM557705
- Tiago Augusto Desiderato - RM558485

## Descrição

NeoMoto é uma API RESTful para gestão da frota da Mottu (motos), organizada por filiais e com histórico de manutenções. Desenvolvida com .NET 9 Minimal API e PostgreSQL.

## Domínio e Entidades

### Filiais

- Unidades físicas que organizam a frota
- Campos: Id, Nome, Endereco, Cidade, UF
- Relacionamento: Uma filial pode ter várias motos

### Motos

- Veículos da frota associados a uma filial
- Campos: Id, Placa (única), Modelo, Ano, FilialId
- Relacionamento: Uma moto pertence a uma filial e pode ter várias manutenções

### Manutenções

- Eventos de manutenção vinculados a uma moto
- Campos: Id, MotoId, Data, Descricao, Custo
- Relacionamento: Uma manutenção pertence a uma moto

## Funcionalidades Implementadas

- CRUD completo para 3 entidades (Create, Read, Update, Delete)
- Paginação com pageNumber e pageSize
- HATEOAS com links relacionais (self, filial, motos, manutencoes)
- Status codes HTTP adequados (200, 201, 204, 400, 404)
- Swagger/OpenAPI com documentação completa
- Validações de dados e integridade referencial
- Relacionamentos entre entidades com cascade/restrict

## Tecnologias

- .NET 9.0 (Minimal API)
- Entity Framework Core 8.0
- PostgreSQL 15
- Docker e Docker Compose
- Swagger/OpenAPI
- xUnit (testes)

## Requisitos

- .NET SDK 9.0 ou superior
- Docker Desktop instalado e rodando
- PowerShell (para scripts automatizados)

## Como Executar

### Opção 1 - Processo Completo Manual

**Passo 1: Iniciar PostgreSQL**

```bash
docker-compose up -d
```

**Passo 2: Restaurar dependências**

```bash
dotnet restore
```

**Passo 3: Compilar o projeto**

```bash
dotnet build
```

**Passo 4: Aplicar migrations no banco**

```bash
dotnet ef database update --project NeoMoto.Infrastructure --startup-project NeoMoto.Api
```

**Passo 5: Executar a API**

```bash
dotnet run --project NeoMoto.Api --urls http://localhost:5010
```

**Passo 6: Acessar documentação**

- Swagger UI: http://localhost:5010/swagger
- API Base URL: http://localhost:5010/api

## Endpoints da API

### Filiais

- `GET /api/filiais` - Listar filiais (com paginação)
- `GET /api/filiais/{id}` - Buscar filial por ID
- `POST /api/filiais` - Criar nova filial
- `PUT /api/filiais/{id}` - Atualizar filial
- `DELETE /api/filiais/{id}` - Deletar filial
- `GET /api/filiais/{id}/motos` - Listar motos de uma filial

### Motos

- `GET /api/motos` - Listar motos (com paginação)
- `GET /api/motos/{id}` - Buscar moto por ID
- `POST /api/motos` - Criar nova moto
- `PUT /api/motos/{id}` - Atualizar moto
- `DELETE /api/motos/{id}` - Deletar moto
- `GET /api/motos/{id}/manutencoes` - Listar manutenções de uma moto

### Manutenções

- `GET /api/manutencoes` - Listar manutenções (com paginação)
- `GET /api/manutencoes/{id}` - Buscar manutenção por ID
- `POST /api/manutencoes` - Criar nova manutenção
- `PUT /api/manutencoes/{id}` - Atualizar manutenção
- `DELETE /api/manutencoes/{id}` - Deletar manutenção

## Exemplos de Uso

### Criar Filial

```bash
POST /api/filiais
Content-Type: application/json

{
  "nome": "Filial Norte",
  "endereco": "Rua das Flores 123",
  "cidade": "Sao Paulo",
  "uf": "SP"
}
```

### Criar Moto

```bash
POST /api/motos
Content-Type: application/json

{
  "placa": "ABC1234",
  "modelo": "Honda CG 160",
  "ano": 2023,
  "filialId": "guid-da-filial"
}
```

### Criar Manutenção

```bash
POST /api/manutencoes
Content-Type: application/json

{
  "motoId": "guid-da-moto",
  "data": "2024-09-24T10:30:00Z",
  "descricao": "Revisao dos 10000 km",
  "custo": 250.00
}
```

### Listar com Paginação

```bash
GET /api/motos?pageNumber=1&pageSize=10
GET /api/filiais?pageNumber=2&pageSize=5
```

## Testes

### Executar todos os testes

```bash
dotnet test
```

### Executar testes com detalhes

```bash
dotnet test --logger console --verbosity normal
```

### Testes implementados

- Smoke tests da API
- Testes de integração com PostgreSQL
- Validação do Swagger/OpenAPI

## Gerenciamento do Docker

### Comandos básicos

```bash
# Iniciar PostgreSQL
docker-compose up -d

# Parar PostgreSQL
docker-compose down

# Ver logs do PostgreSQL
docker-compose logs postgres

# Resetar dados (cuidado - apaga tudo)
docker-compose down -v

# Ver status dos containers
docker-compose ps
```

### Configuração do PostgreSQL

- **Usuário:** neomoto_user
- **Senha:** neomoto_pass123
- **Database:** neomoto
- **Porta:** 5432
- **Volume persistente:** postgres_data

## Testando a API via PowerShell/CMD

### Listar filiais

```powershell
Invoke-RestMethod -Uri "http://localhost:5010/api/filiais" -Method GET
```

### Criar filial

```powershell
$body = '{"nome":"Filial Teste","endereco":"Rua Teste 123","cidade":"Sao Paulo","uf":"SP"}'
Invoke-RestMethod -Uri "http://localhost:5010/api/filiais" -Method POST -Body $body -ContentType "application/json"
```

### Atualizar filial

```powershell
$body = '{"id":"guid-aqui","nome":"Filial Atualizada","endereco":"Rua Nova 456","cidade":"Sao Paulo","uf":"SP"}'
Invoke-RestMethod -Uri "http://localhost:5010/api/filiais/{id}" -Method PUT -Body $body -ContentType "application/json"
```

### Deletar filial

```powershell
Invoke-RestMethod -Uri "http://localhost:5010/api/filiais/{id}" -Method DELETE
```

## Estrutura do Projeto

```
ProjetoNetMottu/
├── NeoMoto.Api/                 # API principal (Minimal API)
├── NeoMoto.Domain/              # Entidades do domínio
├── NeoMoto.Infrastructure/      # DbContext e configurações EF
├── NeoMoto.Tests/              # Testes automatizados
├── docker-compose.yml          # Configuração PostgreSQL
└── README.md                   # Esta documentação
```

## Arquitetura

### Padrão Utilizado

- **Minimal API** para endpoints REST
- **Clean Architecture** com separação de responsabilidades
- **Entity Framework Core** com Code First
- **PostgreSQL** como banco de dados

### Justificativa da Arquitetura

- **Simplicidade:** Minimal API reduz boilerplate
- **Performance:** Menos overhead que controllers tradicionais
- **Manutenibilidade:** Separação clara entre camadas
- **Escalabilidade:** PostgreSQL suporta alta concorrência
- **Portabilidade:** Docker facilita deployment

## Integrantes do Projeto

- **Afonso Correia Pereira** - RM557863
- **Adel Mouhaidly** - RM557705
- **Tiago Augusto Desiderato** - RM558485

## Status do Projeto

- [x] CRUD completo para 3 entidades
- [x] Paginação implementada
- [x] HATEOAS com links relacionais
- [x] Swagger/OpenAPI documentado
- [x] Testes automatizados funcionando
- [x] PostgreSQL integrado
- [x] Docker configurado
- [x] Validações de dados
- [x] Status codes HTTP adequados

## Solução de Problemas

### Erro "Docker não encontrado"

1. Instale o Docker Desktop
2. Certifique-se que está rodando
3. Reinicie o terminal

### Erro de compilação

1. Verifique se tem .NET 9 SDK instalado
2. Execute: `dotnet --version`
3. Execute: `dotnet restore` e `dotnet build`

### Erro de conexão com banco

1. Verifique se PostgreSQL está rodando: `docker-compose ps`
2. Verifique logs: `docker-compose logs postgres`
3. Reaplique migrations: `dotnet ef database update --project NeoMoto.Infrastructure --startup-project NeoMoto.Api`

## Segurança, Versionamento e Health Checks

- Endpoints públicos: `/health`, `/health/ready`, `/swagger/*`
- Endpoints protegidos exigem header: `X-API-KEY: dev-secret-key`
- Versão da API: usar `?api-version=1.0` ou header `x-api-version: 1.0`

## Endpoint ML.NET

- `POST /api/ml/predict` (protegido) — estima custo de manutenção a partir de features:

```
{
  "ageYears": 3,
  "daysSinceLastService": 60,
  "serviceType": 1
}
```

## Executando Testes

- Rodar todos os testes: `dotnet test`
- Inclui testes de integração (WebApplicationFactory) e unitário (xUnit) do estimador ML.NET.
