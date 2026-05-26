// identity.bicep — user-assigned managed identity used by orchestrator and other workloads.

@description('Azure region.')
param location string

@description('Resource tags.')
param tags object

@description('User-assigned managed identity name.')
param identityName string

resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' = {
  name: identityName
  location: location
  tags: tags
}

output identityResourceId string = uami.id
output principalId string = uami.properties.principalId
output clientId string = uami.properties.clientId
output identityName string = uami.name
