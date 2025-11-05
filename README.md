# Sprint 4 - DevOps Tools & Cloud Computing

## Informacoes do Grupo

**Integrantes:**

- Afonso Correia Pereira - RM557863 - 2TDSA
- Adel Mouhaidly - RM557705 - 2TDSA
- Tiago Augusto Desiderato - RM558485 - 2TDSB

## Links do Projeto

- **GitHub:** https://github.com/AdelMouhaidly/NeoMotoDevops.git
- **Azure DevOps:** [Link do Projeto Azure DevOps]
- **YouTube:** https://youtu.be/dIJ2qahu6S8?si=EEYbln0V5JPE7Ei9

## Descricao da Solucao

A **NeoMoto API** e uma solucao completa para gestao de frotas de motocicletas, desenvolvida com .NET 9.0 utilizando Minimal API, PostgreSQL 15 como banco de dados em nuvem (Azure Database for PostgreSQL), Docker para containerizacao, Azure DevOps para CI/CD e Azure Container Instance para hosting da aplicacao.

### Stack de Tecnologias:

- .NET 9.0 (Minimal API)
- PostgreSQL 15 (Azure Database for PostgreSQL Flexible Server)
- Docker
- Azure DevOps (Pipelines CI/CD)
- Azure Container Registry (ACR)
- Azure Container Instance (ACI)
- Entity Framework Core 8.0
- xUnit (testes automatizados)

### Funcionalidades:

- CRUD completo para Filiais, Motos e Manutencoes
- Paginacao de resultados
- HATEOAS com links relacionais
- Swagger/OpenAPI para documentacao da API
- Testes automatizados com xUnit

## Estrutura do Projeto

```
Challenge4Devops2Sem/
├── ProjetoNetMottu/
│   ├── NeoMoto.Api/             # API REST
│   ├── NeoMoto.Domain/          # Entidades de dominio
│   ├── NeoMoto.Infrastructure/  # DbContext e Migrations
│   ├── NeoMoto.Tests/           # Testes automatizados
│   ├── Dockerfile               # Container da aplicacao
│   └── docker-compose.yml       # PostgreSQL local
├── azure-pipelines.yml          # Pipeline CI/CD completa
└── setup-azure-resources.sh     # Script para criar recursos Azure
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

Execute o script para criar todos os recursos necessarios:

```bash
bash setup-azure-resources.sh
```

O script cria automaticamente:

- Resource Group (rg-neomoto-prod)
- Azure Container Registry (neomotoacr)
- Azure Database for PostgreSQL Flexible Server (neomoto-db-neomoto)
- Firewall rules
- Azure Container Instance (neomoto-api-neomoto)

### 2. Configurar Azure DevOps

1. Criar projeto no Azure DevOps (nome: "Sprint 4 - Azure DevOps")
2. Configurar Service Connections:
   - azure-connection (Azure Resource Manager)
   - acr-connection (Docker Registry)
3. Criar Variable Group "neomoto-variables" com variaveis protegidas:
   - acrName
   - acrLoginServer
   - acrUsername
   - acrPassword
   - resourceGroupName
   - containerInstanceName
   - dnsNameLabel
   - dbConnectionString
4. Conectar pipeline ao repositorio GitHub
5. Convidar professor com acesso Basic

### 3. Aplicar Migrations no Banco

```bash
cd ProjetoNetMottu
dotnet ef database update \
  --project NeoMoto.Infrastructure \
  --startup-project NeoMoto.Api \
  --connection "Host=neomoto-db-neomoto.postgres.database.azure.com;Port=5432;Database=neomoto;Username=neomoto_admin;Password=NeoMoto2024!;SslMode=Require"
```

### 4. Executar Pipeline

- Faca um commit na branch master
- Pipeline sera executada automaticamente
- Aguarde deploy no Azure Container Instance

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

## Gerenciamento de Recursos Azure

### Deletar Resource Group Inteiro

Para deletar todos os recursos do projeto (Container Instance, PostgreSQL, ACR, etc):

```bash
az group delete \
  --name rg-neomoto-prod \
  --yes \
  --no-wait
```

**CUIDADO:** Isso deleta TUDO. Aguarde 5-10 minutos para a delecao completar.

Para recriar tudo apos a delecao:

```bash
bash setup-azure-resources.sh
```

---

### Parar/Deletar Recursos para Economizar

#### Opção 1: Deletar apenas o Container Instance (Recomendado)

Para parar a aplicação sem perder os outros recursos (banco de dados, ACR, etc):

```bash
az container delete \
  --resource-group rg-neomoto-prod \
  --name neomoto-api \
  --yes
```

**O que mantem:**

- ACR (imagens Docker preservadas)
- PostgreSQL (banco de dados preservado)
- Todas as configuracoes

**Para rodar novamente:**

- Execute a pipeline novamente (ela vai recriar o Container Instance automaticamente)

---

#### Opção 2: Parar o PostgreSQL (Economiza mais)

Para parar o servidor PostgreSQL (mantém dados, mas para o serviço):

```bash
az postgres flexible-server stop \
  --resource-group rg-neomoto-prod \
  --name neomoto-db-neomoto
```

**Para iniciar novamente:**

```bash
az postgres flexible-server start \
  --resource-group rg-neomoto-prod \
  --name neomoto-db-neomoto
```

---

#### Opção 3: Deletar Resource Group Inteiro (Maxima Economia)

**CUIDADO:** Isso deleta TUDO (Container Instance, PostgreSQL, ACR, etc).

```bash
az group delete \
  --name rg-neomoto-prod \
  --yes \
  --no-wait
```

**O que e perdido:**

- Dados do banco de dados
- Imagens Docker no ACR
- Todas as configuracoes dos recursos

**O que e mantido:**

- Codigo no GitHub
- Pipeline no Azure DevOps
- Service Connections
- Variable Groups

**Para recriar tudo:**

1. Aguarde a deleção terminar (5-10 minutos)
2. Execute o script novamente:
   ```bash
   bash setup-azure-resources.sh
   ```
3. Aplique migrations novamente:
   ```bash
   cd ProjetoNetMottu
   dotnet ef database update \
     --project NeoMoto.Infrastructure \
     --startup-project NeoMoto.Api \
     --connection "Host=neomoto-db-neomoto.postgres.database.azure.com;Port=5432;Database=neomoto;Username=neomoto_admin;Password=NeoMoto2024!;SslMode=Require"
   ```
4. Atualize Variable Group no Azure DevOps (se necessário):
   - Novas credenciais do ACR
   - Nova connection string do banco
5. Execute a pipeline novamente

---

### Verificar Status dos Recursos

```bash
# Ver todos os recursos no Resource Group
az resource list \
  --resource-group rg-neomoto-prod \
  --output table

# Ver status do Container Instance
az container show \
  --resource-group rg-neomoto-prod \
  --name neomoto-api \
  --query "{State:instanceView.state,IP:ipAddress.ip,FQDN:ipAddress.fqdn}" \
  --output table

# Ver status do PostgreSQL
az postgres flexible-server show \
  --resource-group rg-neomoto-prod \
  --name neomoto-db-neomoto \
  --query "{State:state,Version:version,Location:location}" \
  --output table
```

---

### Verificar Custos no Portal Azure

1. Acesse: https://portal.azure.com
2. Vá em **Cost Management + Billing**
3. Veja os custos em tempo real
4. Filtre por Resource Group: `rg-neomoto-prod`

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

## Requisitos Atendidos

- Descricao da solucao
- Diagrama da arquitetura + Fluxo CI/CD
- Detalhamento dos componentes
- Banco de dados em nuvem (PostgreSQL Azure)
- Projeto configurado no Azure DevOps
- Pipeline CI/CD funcionando
- Build + testes automaticos
- Deploy automatico
- Trigger na branch master
- Variaveis de ambiente protegidas
- Artefato gerado e publicado
- Etapa de testes
- Deploy com Docker no ACI
- Video demonstrativo completo

## Licenca

Projeto academico - FIAP 2024
