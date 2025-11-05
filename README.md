# Sprint 4 - DevOps Tools & Cloud Computing

## Informacoes do Grupo

**Integrantes:**
- Afonso Correia Pereira - RM557863 - 2TDSPS
- Adel Mouhaidly - RM557705 - 2TDSPS
- Tiago Augusto Desiderato - RM558485 - 2TDSPS

## Links do Projeto

- **GitHub:** https://github.com/AdelMouhaidly/NeoMotoDevops.git
- **Azure DevOps:** [Link do Projeto Azure DevOps]
- **YouTube:** [Link do Video]

## Descricao da Solucao

A **NeoMoto API** e uma solucao completa para gestao de frotas de motocicletas, desenvolvida com:

- **.NET 9.0** com Minimal API
- **PostgreSQL 15** (Azure Database for PostgreSQL)
- **Docker** para containerizacao
- **Azure DevOps** para CI/CD
- **Azure Container Instance** para hosting

### Funcionalidades:
- CRUD completo para Filiais, Motos e Manutencoes
- Paginacao de resultados
- HATEOAS com links relacionais
- Swagger/OpenAPI
- Testes automatizados

## Estrutura do Projeto

```
Challenge4Devops2Sem/
├── ProjetoNetMottu/              # Projeto .NET
│   ├── NeoMoto.Api/             # API REST
│   ├── NeoMoto.Domain/          # Entidades
│   ├── NeoMoto.Infrastructure/  # DbContext e Migrations
│   ├── NeoMoto.Tests/           # Testes automatizados
│   ├── Dockerfile               # Container da aplicacao
│   └── docker-compose.yml       # PostgreSQL local
├── azure-pipelines.yml          # Pipeline CI/CD completa
├── INSTRUCOES_AZURE_DEVOPS.md   # Guia de configuracao
├── TEMPLATE_PDF.md              # Template para documentacao
└── azure-variables-template.txt # Variaveis do Azure DevOps
```

## Pipeline CI/CD

### CI (Continuous Integration)
- Trigger automatico na branch master
- Build com .NET 9
- Testes automatizados (xUnit)
- Criacao de imagem Docker
- Publicacao de artefatos

### CD (Continuous Deployment)
- Deploy automatico apos CI
- Push para Azure Container Registry
- Deploy no Azure Container Instance (ACI)
- Variaveis de ambiente protegidas

## Como Usar Este Projeto

### 1. Configurar Recursos no Azure

Execute os comandos no arquivo `azure-variables-template.txt` para criar:
- Resource Group
- Azure Container Registry
- Azure Database for PostgreSQL

### 2. Configurar Azure DevOps

Siga as instrucoes em `INSTRUCOES_AZURE_DEVOPS.md`:
- Criar projeto no Azure DevOps
- Configurar service connections
- Configurar variaveis protegidas
- Conectar ao GitHub
- Convidar o professor

### 3. Aplicar Migrations no Banco

```powershell
cd ProjetoNetMottu
.\run-migrations-azure.ps1 -ServerName "neomoto-db-server" -Username "neomoto_admin" -Password "SuaSenha"
```

### 4. Executar Pipeline

- Faca um commit na branch master
- Pipeline sera executada automaticamente
- Aguarde deploy no Azure Container Instance

### 5. Testar a API

```powershell
.\test-crud.ps1 -BaseUrl "http://[seu-dns].brazilsouth.azurecontainer.io:8080"
```

## Desenvolvimento Local

### Requisitos:
- .NET SDK 9.0
- Docker Desktop
- PowerShell

### Executar localmente:

```powershell
cd ProjetoNetMottu

# Iniciar PostgreSQL
docker-compose up -d

# Restaurar e compilar
dotnet restore
dotnet build

# Aplicar migrations
dotnet ef database update --project NeoMoto.Infrastructure --startup-project NeoMoto.Api

# Executar API
dotnet run --project NeoMoto.Api
```

Acesse: http://localhost:5010/swagger

## Testes

```powershell
cd ProjetoNetMottu
dotnet test
```

## Tecnologias Utilizadas

### Backend:
- .NET 9.0 (Minimal API)
- Entity Framework Core 8.0
- PostgreSQL 15

### DevOps:
- Azure DevOps Pipelines
- Docker
- Azure Container Registry
- Azure Container Instance
- Azure Database for PostgreSQL

### Testes:
- xUnit
- FluentAssertions
- Microsoft.AspNetCore.Mvc.Testing

## Documentacao da API

### Endpoints Principais:

**Filiais:**
- GET /api/filiais
- GET /api/filiais/{id}
- POST /api/filiais
- PUT /api/filiais/{id}
- DELETE /api/filiais/{id}

**Motos:**
- GET /api/motos
- GET /api/motos/{id}
- POST /api/motos
- PUT /api/motos/{id}
- DELETE /api/motos/{id}

**Manutencoes:**
- GET /api/manutencoes
- GET /api/manutencoes/{id}
- POST /api/manutencoes
- PUT /api/manutencoes/{id}
- DELETE /api/manutencoes/{id}

Documentacao completa: `/swagger`

## Banco de Dados

O projeto utiliza **Azure Database for PostgreSQL** (PaaS) conforme requisitos:
- Servico gerenciado
- Backup automatico
- SSL obrigatorio
- Alta disponibilidade

Connection strings sao gerenciadas via variaveis de ambiente protegidas no Azure DevOps.

## Variaveis de Ambiente

Todas as credenciais e connection strings estao protegidas:
- Configuradas no Azure DevOps Library
- Marcadas como secretas
- Nunca commitadas no codigo
- Injetadas durante o deploy

## Video Demonstrativo

O video cobre:
1. Apresentacao das ferramentas
2. Configuracao da pipeline
3. Alteracao do README e push
4. Trigger automatico da pipeline
5. Explicacao de cada etapa CI/CD
6. Artefato criado e testes
7. Recursos no Portal Azure
8. CRUD completo com validacao no banco

## Criterios de Avaliacao Atendidos

- ✅ Descricao da solucao
- ✅ Diagrama da arquitetura + Fluxo CI/CD
- ✅ Detalhamento dos componentes
- ✅ Banco de dados em nuvem (PostgreSQL Azure)
- ✅ Projeto configurado no Azure DevOps
- ✅ Professor convidado com acesso Basic
- ✅ Pipeline CI/CD funcionando
- ✅ Build + testes automaticos
- ✅ Deploy automatico
- ✅ Trigger na branch master
- ✅ Variaveis de ambiente protegidas
- ✅ Artefato gerado e publicado
- ✅ Etapa de testes
- ✅ Deploy com Docker no ACI
- ✅ Video demonstrativo completo

## Suporte

Para problemas ou duvidas:
1. Verificar `INSTRUCOES_AZURE_DEVOPS.md`
2. Revisar logs da pipeline
3. Validar variaveis de ambiente
4. Conferir connection string do banco

## Licenca

Projeto academico - FIAP 2024

