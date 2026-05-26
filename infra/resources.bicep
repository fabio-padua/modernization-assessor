// resources.bicep — resource-group-scope composition for Modernization Assessor.
// Orchestrates observability, identity, ACR, Foundry, and the Container Apps Job.

targetScope = 'resourceGroup'

@description('Partner short code.')
param partnerCode string

@description('Environment name (dev, stg, prod).')
param environmentName string

@description('Azure region.')
param location string = resourceGroup().location

@description('Common tags.')
param tags object

@description('Foundry model deployment name.')
param modelDeploymentName string

@description('Foundry model name.')
param modelName string

@description('Foundry model version. Empty string means default.')
param modelVersion string

@description('Foundry model deployment capacity.')
param modelCapacity int

@description('Initial container image for the orchestrator job.')
param orchestratorImage string

@description('Optional principal id granted operator roles on the resources.')
param operatorPrincipalId string

// ---------- Unique suffix for globally unique resource names ----------
var nameSuffix = toLower(substring(uniqueString(resourceGroup().id, partnerCode, environmentName), 0, 6))
var baseName = 'modassessor-${partnerCode}-${environmentName}'

// ---------- Observability ----------
module observability 'modules/observability.bicep' = {
  name: 'observability'
  params: {
    location: location
    tags: tags
    logAnalyticsName: 'log-${baseName}'
    appInsightsName: 'appi-${baseName}'
  }
}

// ---------- Managed identity ----------
module identity 'modules/identity.bicep' = {
  name: 'identity'
  params: {
    location: location
    tags: tags
    identityName: 'id-${baseName}'
  }
}

// ---------- Container Registry ----------
module registry 'modules/registry.bicep' = {
  name: 'registry'
  params: {
    location: location
    tags: tags
    registryName: 'acr${replace(baseName, '-', '')}${nameSuffix}'
    identityPrincipalId: identity.outputs.principalId
    operatorPrincipalId: operatorPrincipalId
  }
}

// ---------- Foundry ----------
module foundry 'modules/foundry.bicep' = {
  name: 'foundry'
  params: {
    location: location
    tags: tags
    foundryAccountName: 'aifoundry-${baseName}-${nameSuffix}'
    foundryProjectName: 'modassessor'
    modelDeploymentName: modelDeploymentName
    modelName: modelName
    modelVersion: modelVersion
    modelCapacity: modelCapacity
    identityPrincipalId: identity.outputs.principalId
    operatorPrincipalId: operatorPrincipalId
  }
}

// ---------- Container Apps (orchestrator job) ----------
module containerapps 'modules/containerapps.bicep' = {
  name: 'containerapps'
  params: {
    location: location
    tags: tags
    environmentName: 'cae-${baseName}'
    orchestratorJobName: 'caj-${baseName}-orchestrator'
    logAnalyticsCustomerId: observability.outputs.logAnalyticsCustomerId
    logAnalyticsSharedKey: observability.outputs.logAnalyticsSharedKey
    appInsightsConnectionString: observability.outputs.appInsightsConnectionString
    identityResourceId: identity.outputs.identityResourceId
    identityClientId: identity.outputs.clientId
    containerRegistryServer: registry.outputs.loginServer
    orchestratorImage: orchestratorImage
    projectEndpoint: foundry.outputs.projectEndpoint
    modelDeploymentName: modelDeploymentName
  }
}

// ---------- Outputs ----------
output containerRegistryName string = registry.outputs.name
output containerRegistryEndpoint string = registry.outputs.loginServer
output containerAppsEnvironmentName string = containerapps.outputs.environmentName
output orchestratorJobName string = containerapps.outputs.orchestratorJobName
output foundryAccountName string = foundry.outputs.accountName
output projectEndpoint string = foundry.outputs.projectEndpoint
output modelDeploymentName string = modelDeploymentName
output logAnalyticsWorkspaceId string = observability.outputs.logAnalyticsResourceId
output appInsightsConnectionString string = observability.outputs.appInsightsConnectionString
output identityResourceId string = identity.outputs.identityResourceId
output identityClientId string = identity.outputs.clientId
