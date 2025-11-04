#!/bin/bash

# ==============================================================================
# Script para criar todos os recursos Azure para o projeto NeoMoto
# ==============================================================================

set -e  # Para em caso de erro

echo "================================"
echo "NeoMoto - Setup Azure Resources"
echo "================================"
echo ""

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# ==============================================================================
# 1. COLETAR INFORMACOES
# ==============================================================================

echo -e "${CYAN}Passo 1: Configuracao Inicial${NC}"
echo ""

read -p "Digite um nome UNICO para seus recursos (ex: seunome): " NOME_UNICO
if [ -z "$NOME_UNICO" ]; then
    echo -e "${RED}Nome nao pode ser vazio!${NC}"
    exit 1
fi

# Variaveis
RESOURCE_GROUP="rg-neomoto-prod"
LOCATION="brazilsouth"
ACR_NAME="${NOME_UNICO}acr"
POSTGRES_SERVER="neomoto-db-${NOME_UNICO}"
POSTGRES_USER="neomoto_admin"
POSTGRES_PASSWORD="NeoMoto2024!"
POSTGRES_DATABASE="neomoto"

echo ""
echo -e "${YELLOW}Recursos que serao criados:${NC}"
echo "  - Resource Group: $RESOURCE_GROUP"
echo "  - ACR: $ACR_NAME"
echo "  - PostgreSQL Server: $POSTGRES_SERVER"
echo "  - Database: $POSTGRES_DATABASE"
echo "  - Location: $LOCATION"
echo ""

read -p "Deseja continuar? (s/n): " CONFIRM
if [ "$CONFIRM" != "s" ]; then
    echo "Operacao cancelada."
    exit 0
fi

echo ""

# ==============================================================================
# 2. VERIFICAR LOGIN NO AZURE
# ==============================================================================

echo -e "${CYAN}Passo 2: Verificando login no Azure...${NC}"
if ! az account show &> /dev/null; then
    echo -e "${YELLOW}Voce nao esta logado. Fazendo login...${NC}"
    az login
else
    echo -e "${GREEN}Ja esta logado no Azure${NC}"
    az account show --query "{Subscription:name, ID:id}" --output table
fi

echo ""
read -p "Esta subscription esta correta? (s/n): " CONFIRM_SUB
if [ "$CONFIRM_SUB" != "s" ]; then
    echo "Execute 'az account list' e 'az account set --subscription ID' para trocar"
    exit 0
fi

echo ""

# ==============================================================================
# 3. CRIAR RESOURCE GROUP
# ==============================================================================

echo -e "${CYAN}Passo 3: Criando Resource Group...${NC}"

if az group show --name $RESOURCE_GROUP &> /dev/null; then
    echo -e "${YELLOW}Resource Group ja existe${NC}"
else
    az group create \
        --name $RESOURCE_GROUP \
        --location $LOCATION \
        --output table
    echo -e "${GREEN}Resource Group criado!${NC}"
fi

echo ""

# ==============================================================================
# 4. CRIAR AZURE CONTAINER REGISTRY
# ==============================================================================

echo -e "${CYAN}Passo 4: Criando Azure Container Registry...${NC}"

if az acr show --name $ACR_NAME --resource-group $RESOURCE_GROUP &> /dev/null; then
    echo -e "${YELLOW}ACR ja existe${NC}"
else
    az acr create \
        --resource-group $RESOURCE_GROUP \
        --name $ACR_NAME \
        --sku Basic \
        --output table
    echo -e "${GREEN}ACR criado!${NC}"
fi

# Habilitar admin user
echo "Habilitando admin user no ACR..."
az acr update \
    --name $ACR_NAME \
    --admin-enabled true \
    --output none

echo -e "${GREEN}ACR configurado!${NC}"
echo ""

# ==============================================================================
# 5. OBTER CREDENCIAIS DO ACR
# ==============================================================================

echo -e "${CYAN}Passo 5: Obtendo credenciais do ACR...${NC}"

ACR_USERNAME=$(az acr credential show --name $ACR_NAME --query "username" --output tsv)
ACR_PASSWORD=$(az acr credential show --name $ACR_NAME --query "passwords[0].value" --output tsv)
ACR_LOGIN_SERVER="${ACR_NAME}.azurecr.io"

echo -e "${GREEN}Credenciais obtidas!${NC}"
echo ""

# ==============================================================================
# 6. CRIAR POSTGRESQL
# ==============================================================================

echo -e "${CYAN}Passo 6: Criando PostgreSQL Flexible Server...${NC}"
echo "Isso pode levar 5-10 minutos..."

if az postgres flexible-server show --name $POSTGRES_SERVER --resource-group $RESOURCE_GROUP &> /dev/null; then
    echo -e "${YELLOW}PostgreSQL Server ja existe${NC}"
else
    az postgres flexible-server create \
        --name $POSTGRES_SERVER \
        --resource-group $RESOURCE_GROUP \
        --location $LOCATION \
        --admin-user $POSTGRES_USER \
        --admin-password $POSTGRES_PASSWORD \
        --sku-name Standard_B1ms \
        --tier Burstable \
        --storage-size 32 \
        --version 15 \
        --public-access 0.0.0.0 \
        --yes \
        --output table
    
    echo -e "${GREEN}PostgreSQL Server criado!${NC}"
fi

echo ""

# ==============================================================================
# 7. CRIAR DATABASE
# ==============================================================================

echo -e "${CYAN}Passo 7: Criando Database...${NC}"

if az postgres flexible-server db show \
    --server-name $POSTGRES_SERVER \
    --resource-group $RESOURCE_GROUP \
    --database-name $POSTGRES_DATABASE &> /dev/null; then
    echo -e "${YELLOW}Database ja existe${NC}"
else
    az postgres flexible-server db create \
        --resource-group $RESOURCE_GROUP \
        --server-name $POSTGRES_SERVER \
        --database-name $POSTGRES_DATABASE \
        --output table
    
    echo -e "${GREEN}Database criada!${NC}"
fi

echo ""

# ==============================================================================
# 8. CONFIGURAR FIREWALL
# ==============================================================================

echo -e "${CYAN}Passo 8: Configurando Firewall do PostgreSQL...${NC}"

# Permitir servicos Azure
if az postgres flexible-server firewall-rule show \
    --name AllowAzureServices \
    --server-name $POSTGRES_SERVER \
    --resource-group $RESOURCE_GROUP &> /dev/null; then
    echo -e "${YELLOW}Regra AllowAzureServices ja existe${NC}"
else
    az postgres flexible-server firewall-rule create \
        --resource-group $RESOURCE_GROUP \
        --name $POSTGRES_SERVER \
        --rule-name AllowAzureServices \
        --start-ip-address 0.0.0.0 \
        --end-ip-address 0.0.0.0 \
        --output table
    
    echo -e "${GREEN}Firewall configurado!${NC}"
fi

echo ""

# ==============================================================================
# 9. GERAR CONNECTION STRING
# ==============================================================================

POSTGRES_HOST="${POSTGRES_SERVER}.postgres.database.azure.com"
DB_CONNECTION_STRING="Host=${POSTGRES_HOST};Port=5432;Database=${POSTGRES_DATABASE};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD};SslMode=Require"

# ==============================================================================
# 10. RESUMO E SALVAR CREDENCIAIS
# ==============================================================================

echo ""
echo -e "${GREEN}================================${NC}"
echo -e "${GREEN}RECURSOS CRIADOS COM SUCESSO!${NC}"
echo -e "${GREEN}================================${NC}"
echo ""

# Criar arquivo com as credenciais
CREDENTIALS_FILE="azure-credentials-${NOME_UNICO}.txt"

cat > $CREDENTIALS_FILE << EOF
================================
CREDENCIAIS AZURE - NeoMoto
================================
Criado em: $(date)

RESOURCE GROUP:
  Nome: $RESOURCE_GROUP
  Location: $LOCATION

AZURE CONTAINER REGISTRY:
  Nome: $ACR_NAME
  Login Server: $ACR_LOGIN_SERVER
  Username: $ACR_USERNAME
  Password: $ACR_PASSWORD

POSTGRESQL:
  Server: $POSTGRES_HOST
  Database: $POSTGRES_DATABASE
  Username: $POSTGRES_USER
  Password: $POSTGRES_PASSWORD
  
CONNECTION STRING:
$DB_CONNECTION_STRING

================================
VARIAVEIS PARA AZURE DEVOPS
================================

PUBLICAS:
  resourceGroupName = $RESOURCE_GROUP
  containerInstanceName = neomoto-api
  dnsNameLabel = neomoto-api-${NOME_UNICO}
  acrName = $ACR_NAME
  acrLoginServer = $ACR_LOGIN_SERVER
  dockerImageName = neomoto-api

SECRETAS (marcar cadeado):
  azureSubscription = azure-connection
  dockerRegistryServiceConnection = acr-connection
  dbConnectionString = $DB_CONNECTION_STRING
  acrUsername = $ACR_USERNAME
  acrPassword = $ACR_PASSWORD

================================
PROXIMOS PASSOS
================================

1. APLICAR MIGRATIONS:
   cd ProjetoNetMottu
   dotnet ef database update --project NeoMoto.Infrastructure --startup-project NeoMoto.Api --connection "$DB_CONNECTION_STRING"

2. CONFIGURAR AZURE DEVOPS:
   - Criar Service Connections (Azure + Docker)
   - Criar Variable Group com as variaveis acima
   - Conectar pipeline ao GitHub
   - Vincular Variable Group a pipeline

3. FAZER DEPLOY:
   git add .
   git commit -m "Add CI/CD pipeline"
   git push origin master

4. ACESSAR APLICACAO:
   URL: http://neomoto-api-${NOME_UNICO}.brazilsouth.azurecontainer.io:8080
   Swagger: http://neomoto-api-${NOME_UNICO}.brazilsouth.azurecontainer.io:8080/swagger

================================
EOF

echo -e "${CYAN}INFORMACOES IMPORTANTES:${NC}"
echo ""
echo "Resource Group: $RESOURCE_GROUP"
echo "ACR: $ACR_LOGIN_SERVER"
echo "PostgreSQL: $POSTGRES_HOST"
echo ""
echo -e "${YELLOW}Todas as credenciais foram salvas em:${NC}"
echo -e "${GREEN}$CREDENTIALS_FILE${NC}"
echo ""
echo -e "${CYAN}Proximos passos:${NC}"
echo "1. Aplicar migrations no banco de dados"
echo "2. Configurar Azure DevOps (Service Connections + Variables)"
echo "3. Executar pipeline"
echo ""

# Mostrar credenciais importantes
echo -e "${YELLOW}=== COPIE ESTAS CREDENCIAIS ===${NC}"
echo ""
echo "ACR Username: $ACR_USERNAME"
echo "ACR Password: $ACR_PASSWORD"
echo ""
echo "DB Connection String:"
echo "$DB_CONNECTION_STRING"
echo ""
echo -e "${YELLOW}===============================${NC}"
echo ""

echo -e "${GREEN}Setup concluido!${NC}"
echo ""

