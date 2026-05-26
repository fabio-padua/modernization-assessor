// foundry.bicep — Azure AI Foundry account + project + model deployment + RBAC.

@description('Azure region.')
param location string

@description('Resource tags.')
param tags object

@description('Foundry (Cognitive Services / AIServices) account name.')
param foundryAccountName string

@description('Foundry project name.')
param foundryProjectName string

@description('Model deployment name (referenced by the orchestrator).')
param modelDeploymentName string

@description('Model name (e.g. gpt-4.1).')
param modelName string

@description('Model version. Empty means use the default version.')
param modelVersion string

@description('Capacity (kTPM units) for the model deployment.')
param modelCapacity int

@description('Principal ID of the workload identity that needs model inference access.')
param identityPrincipalId string

@description('Optional operator principal ID granted contributor on Foundry account.')
param operatorPrincipalId string

@description('SKU for the model deployment.')
param skuName string = 'GlobalStandard'

// Cognitive Services OpenAI User — required to call deployments.
var cogServicesOpenAIUserRoleId = '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
// Azure AI User — required for project-scope operations.
var aiUserRoleId = '53ca6127-db72-4b80-b1b0-d745d6d5456d'
// Cognitive Services Contributor — operator-level Foundry control.
var cogServicesContributorRoleId = '25fbc0a9-bd7c-42a3-aa1a-3b75d497ee68'

resource foundry 'Microsoft.CognitiveServices/accounts@2025-04-01-preview' = {
  name: foundryAccountName
  location: location
  tags: tags
  kind: 'AIServices'
  sku: {
    name: 'S0'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    customSubDomainName: foundryAccountName
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
    allowProjectManagement: true
  }
}

resource project 'Microsoft.CognitiveServices/accounts/projects@2025-04-01-preview' = {
  parent: foundry
  name: foundryProjectName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {}
}

resource modelDeployment 'Microsoft.CognitiveServices/accounts/deployments@2025-04-01-preview' = {
  parent: foundry
  name: modelDeploymentName
  sku: {
    name: skuName
    capacity: modelCapacity
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: modelName
      version: empty(modelVersion) ? null : modelVersion
    }
    versionUpgradeOption: 'OnceCurrentVersionExpired'
    raiPolicyName: 'Microsoft.DefaultV2'
  }
}

resource workloadOpenAIUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(foundry.id, identityPrincipalId, cogServicesOpenAIUserRoleId)
  scope: foundry
  properties: {
    principalId: identityPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', cogServicesOpenAIUserRoleId)
  }
}

resource workloadAIUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(foundry.id, identityPrincipalId, aiUserRoleId)
  scope: foundry
  properties: {
    principalId: identityPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', aiUserRoleId)
  }
}

resource operatorContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(operatorPrincipalId)) {
  name: guid(foundry.id, operatorPrincipalId, cogServicesContributorRoleId)
  scope: foundry
  properties: {
    principalId: operatorPrincipalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', cogServicesContributorRoleId)
  }
}

output accountName string = foundry.name
output accountResourceId string = foundry.id
output projectName string = project.name
output projectEndpoint string = 'https://${foundry.name}.services.ai.azure.com/api/projects/${project.name}'
output modelDeploymentResourceId string = modelDeployment.id
