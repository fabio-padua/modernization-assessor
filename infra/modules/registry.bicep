// registry.bicep — Azure Container Registry with AcrPull granted to the workload identity.

@description('Azure region.')
param location string

@description('Resource tags.')
param tags object

@description('Registry name (must be globally unique, 5-50 alphanumeric).')
param registryName string

@description('Principal ID of the workload identity that needs AcrPull.')
param identityPrincipalId string

@description('Optional operator principal ID granted AcrPush (e.g. deployment service principal).')
param operatorPrincipalId string

@description('SKU for ACR.')
@allowed([
  'Basic'
  'Standard'
  'Premium'
])
param sku string = 'Standard'

var acrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d'
var acrPushRoleId = '8311e382-0749-4cb8-b61a-304f252e45ec'

resource acr 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' = {
  name: registryName
  location: location
  tags: tags
  sku: {
    name: sku
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
    anonymousPullEnabled: false
  }
}

resource acrPullAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acr.id, identityPrincipalId, acrPullRoleId)
  scope: acr
  properties: {
    principalId: identityPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId)
  }
}

resource acrPushAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(operatorPrincipalId)) {
  name: guid(acr.id, operatorPrincipalId, acrPushRoleId)
  scope: acr
  properties: {
    principalId: operatorPrincipalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPushRoleId)
  }
}

output name string = acr.name
output loginServer string = acr.properties.loginServer
output resourceId string = acr.id
