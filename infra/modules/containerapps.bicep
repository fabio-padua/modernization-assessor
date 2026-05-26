// containerapps.bicep — Container Apps Environment + Orchestrator Job.
// The orchestrator is a run-to-completion CLI workload, so we deploy it as a Container Apps Job
// (manual trigger). A future v0.3 HTTP host can be added as a Container App alongside this job.

@description('Azure region.')
param location string

@description('Resource tags.')
param tags object

@description('Container Apps Environment name.')
param environmentName string

@description('Container Apps Job name for the orchestrator.')
param orchestratorJobName string

@description('Log Analytics workspace customer id (GUID).')
param logAnalyticsCustomerId string

@description('Log Analytics workspace shared key.')
@secure()
param logAnalyticsSharedKey string

@description('Application Insights connection string passed to the orchestrator.')
param appInsightsConnectionString string

@description('User-assigned managed identity resource id.')
param identityResourceId string

@description('User-assigned managed identity client id (used by Azure.Identity).')
param identityClientId string

@description('Container Registry login server (e.g. acrxxxx.azurecr.io). Empty disables registry config.')
param containerRegistryServer string

@description('Initial container image for the orchestrator.')
param orchestratorImage string

@description('Foundry project endpoint passed to the orchestrator.')
param projectEndpoint string

@description('Foundry model deployment name passed to the orchestrator.')
param modelDeploymentName string

@description('Reports blob container URL (e.g. https://<acct>.blob.core.windows.net/reports). Empty disables blob publishing.')
param reportsContainerUrl string = ''

@description('Replica timeout in seconds for the orchestrator job (default 30 minutes).')
param replicaTimeoutSeconds int = 1800

@description('Job parallelism.')
param parallelism int = 1

@description('Job replica retry limit.')
param replicaRetryLimit int = 1

resource env 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: environmentName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsCustomerId
        sharedKey: logAnalyticsSharedKey
      }
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}

resource orchestratorJob 'Microsoft.App/jobs@2024-03-01' = {
  name: orchestratorJobName
  location: location
  tags: union(tags, {
    'azd-service-name': 'orchestrator'
  })
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identityResourceId}': {}
    }
  }
  properties: {
    environmentId: env.id
    workloadProfileName: 'Consumption'
    configuration: {
      triggerType: 'Manual'
      replicaTimeout: replicaTimeoutSeconds
      replicaRetryLimit: replicaRetryLimit
      manualTriggerConfig: {
        parallelism: parallelism
        replicaCompletionCount: parallelism
      }
      registries: empty(containerRegistryServer) ? [] : [
        {
          server: containerRegistryServer
          identity: identityResourceId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'orchestrator'
          image: orchestratorImage
          args: [
            '--mode'
            'agents'
            '--inventory'
            '/app/samples/inventory/contoso.csv'
            '--customer'
            'Sample Customer'
          ]
          resources: {
            cpu: json('1.0')
            memory: '2.0Gi'
          }
          env: [
            {
              name: 'Foundry__ProjectEndpoint'
              value: projectEndpoint
            }
            {
              name: 'Foundry__ModelDeploymentName'
              value: modelDeploymentName
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appInsightsConnectionString
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: identityClientId
            }
            {
              name: 'ASSESSOR_OUTPUT_DIR'
              value: '/app/out'
            }
            {
              name: 'ASSESSOR_OUTPUT_BLOB_URL'
              value: reportsContainerUrl
            }
          ]
        }
      ]
    }
  }
}

output environmentName string = env.name
output environmentResourceId string = env.id
output orchestratorJobName string = orchestratorJob.name
output orchestratorJobResourceId string = orchestratorJob.id
