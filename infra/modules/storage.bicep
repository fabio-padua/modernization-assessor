// storage.bicep — Storage account + private blob container used by the orchestrator
// to publish assessment reports. AAD-only (no shared keys), AcrPull-style RBAC via
// Storage Blob Data Contributor on the User-Assigned Managed Identity.

@description('Azure region.')
param location string

@description('Resource tags.')
param tags object

@description('Globally unique storage account name (3-24 chars, lowercase, alphanumeric).')
@minLength(3)
@maxLength(24)
param storageAccountName string

@description('Blob container that holds assessment reports.')
param reportsContainerName string = 'reports'

@description('Principal id of the workload User-Assigned Managed Identity (granted Storage Blob Data Contributor).')
param workloadIdentityPrincipalId string

@description('Optional operator principal id (also granted Storage Blob Data Contributor).')
param operatorPrincipalId string = ''

// Storage Blob Data Contributor — built-in role allowing read/write/delete on blob data.
var blobDataContributorRoleId = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    defaultToOAuthAuthentication: true
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
    encryption: {
      services: {
        blob: {
          enabled: true
          keyType: 'Account'
        }
      }
      keySource: 'Microsoft.Storage'
    }
  }
}

resource blobs 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storage
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
    containerDeleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

resource reportsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobs
  name: reportsContainerName
  properties: {
    publicAccess: 'None'
  }
}

resource workloadRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storage
  name: guid(storage.id, workloadIdentityPrincipalId, blobDataContributorRoleId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', blobDataContributorRoleId)
    principalId: workloadIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource operatorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(operatorPrincipalId)) {
  scope: storage
  name: guid(storage.id, operatorPrincipalId, blobDataContributorRoleId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', blobDataContributorRoleId)
    principalId: operatorPrincipalId
  }
}

output storageAccountName string = storage.name
output blobServiceEndpoint string = storage.properties.primaryEndpoints.blob
output reportsContainerName string = reportsContainer.name
output reportsContainerUrl string = '${storage.properties.primaryEndpoints.blob}${reportsContainer.name}'
