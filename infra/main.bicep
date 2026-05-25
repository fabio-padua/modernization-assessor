// main.bicep — partner-reusable deployment for Modernization Assessor
// v0.1 stub: declares structure, references modules to be filled in v0.2.
//
// Deploy:
//   az deployment sub create -l northcentralus -f infra/main.bicep \
//      -p partnerCode=acme environmentName=dev
//
// Resources provisioned in v0.2:
//   - Resource Group
//   - Azure AI Foundry account + project + capability host
//   - Model deployment (gpt-4.1)
//   - Azure Container Registry
//   - Azure Container Apps environment + orchestrator app
//   - Azure AI Search (RAG: WAF + migration KB)
//   - Cosmos DB (thread state)
//   - Azure API Management (AI Gateway)
//   - Application Insights + Log Analytics workspace
//   - Key Vault
//   - User-assigned Managed Identity + RBAC

targetScope = 'subscription'

@description('Short partner identifier — used in resource names (e.g. acme, contoso, swo).')
@minLength(2)
@maxLength(8)
param partnerCode string

@description('Environment name: dev, stg, prod.')
@allowed(['dev', 'stg', 'prod'])
param environmentName string = 'dev'

@description('Azure region. North Central US is required for Foundry hosted agents preview.')
param location string = 'northcentralus'

@description('Model deployment name to provision in Foundry.')
param modelDeploymentName string = 'gpt-4.1'

@description('Resource tags applied to all resources.')
param tags object = {
  workload: 'modernization-assessor'
  partner: partnerCode
  env: environmentName
  managedBy: 'azd'
}

var rgName = 'rg-modassessor-${partnerCode}-${environmentName}'

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: rgName
  location: location
  tags: tags
}

// modules/foundry.bicep        — Foundry account + project + model deployment
// modules/containerapps.bicep  — ACA environment + orchestrator app
// modules/apim.bicep           — APIM AI Gateway + policies
// modules/aisearch.bicep       — Azure AI Search
// modules/cosmos.bicep         — Cosmos DB for state
// modules/observability.bicep  — Log Analytics + App Insights
// modules/identity.bicep       — User-assigned identity + RBAC

output resourceGroupName string = rg.name
output partnerCode string = partnerCode
output environmentName string = environmentName
