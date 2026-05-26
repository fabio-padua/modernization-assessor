// main.bicep — subscription-scope entry for Modernization Assessor v0.2 deployment.
// Creates the resource group, then deploys all platform resources via resources.bicep.
//
// Local deploy:
//   az deployment sub create -l northcentralus -f infra/main.bicep \
//      -p infra/main.parameters.json
//
// Via azd:
//   azd up

targetScope = 'subscription'

@description('Short partner identifier — used in resource names (e.g. acme, contoso, swo).')
@minLength(2)
@maxLength(8)
param partnerCode string

@description('Environment name: dev, stg, prod.')
@allowed([
  'dev'
  'stg'
  'prod'
])
param environmentName string = 'dev'

@description('Azure region. North Central US is the recommended region for Foundry hosted features.')
param location string = 'northcentralus'

@description('Model deployment name to provision in Foundry.')
param modelDeploymentName string = 'gpt-4.1'

@description('Model name to deploy.')
param modelName string = 'gpt-4.1'

@description('Model version (set to specific version or leave blank to use default).')
param modelVersion string = ''

@description('Capacity (TPM units of 1,000 tokens) for the model deployment.')
@minValue(1)
param modelCapacity int = 50

@description('Initial container image for the orchestrator job. Pipeline overwrites this after build/push.')
param orchestratorImage string = 'mcr.microsoft.com/k8se/quickstart-jobs:latest'

@description('Principal ID granted operator access to the deployed resources (your user object id, or service principal).')
param operatorPrincipalId string = ''

@description('Common resource tags.')
param tags object = {
  workload: 'modernization-assessor'
  partner: partnerCode
  env: environmentName
  managedBy: 'azd'
}

var resourceGroupName = 'rg-modassessor-${partnerCode}-${environmentName}'

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

module platform 'resources.bicep' = {
  name: 'modassessor-platform'
  scope: rg
  params: {
    partnerCode: partnerCode
    environmentName: environmentName
    location: location
    tags: tags
    modelDeploymentName: modelDeploymentName
    modelName: modelName
    modelVersion: modelVersion
    modelCapacity: modelCapacity
    orchestratorImage: orchestratorImage
    operatorPrincipalId: operatorPrincipalId
  }
}

output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_PARTNER_CODE string = partnerCode
output AZURE_ENVIRONMENT_NAME string = environmentName

output AZURE_CONTAINER_REGISTRY_NAME string = platform.outputs.containerRegistryName
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = platform.outputs.containerRegistryEndpoint
output AZURE_CONTAINER_APPS_ENVIRONMENT_NAME string = platform.outputs.containerAppsEnvironmentName
output ORCHESTRATOR_JOB_NAME string = platform.outputs.orchestratorJobName
output AZURE_AI_FOUNDRY_NAME string = platform.outputs.foundryAccountName
output AZURE_AI_PROJECT_ENDPOINT string = platform.outputs.projectEndpoint
output AZURE_AI_MODEL_DEPLOYMENT_NAME string = platform.outputs.modelDeploymentName
output AZURE_LOG_ANALYTICS_WORKSPACE_ID string = platform.outputs.logAnalyticsWorkspaceId
output APPLICATIONINSIGHTS_CONNECTION_STRING string = platform.outputs.appInsightsConnectionString
output AZURE_USER_ASSIGNED_IDENTITY_ID string = platform.outputs.identityResourceId
output AZURE_USER_ASSIGNED_IDENTITY_CLIENT_ID string = platform.outputs.identityClientId
output AZURE_STORAGE_ACCOUNT_NAME string = platform.outputs.storageAccountName
output ASSESSOR_OUTPUT_BLOB_URL string = platform.outputs.reportsContainerUrl
