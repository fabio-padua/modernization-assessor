# Deployment Runbook

This runbook explains how to deploy the Modernization Assessor v0.2 stack to Azure and how the CI/CD pipeline operates.

## 1. What gets deployed

The deployment provisions the following resources into a partner-/environment-scoped resource group `rg-modassessor-<partnerCode>-<environmentName>`:

- Azure Log Analytics workspace
- Azure Application Insights (workspace-based)
- User-assigned Managed Identity
- Azure Container Registry (Standard SKU)
- Azure AI Foundry account, project, and model deployment
- Azure Container Apps Environment
- Azure Container Apps **Job** for the orchestrator CLI workload (manual trigger)

Bicep entry point: [infra/main.bicep](../infra/main.bicep)

Resource composition: [infra/resources.bicep](../infra/resources.bicep)

Modules:
- [infra/modules/observability.bicep](../infra/modules/observability.bicep)
- [infra/modules/identity.bicep](../infra/modules/identity.bicep)
- [infra/modules/registry.bicep](../infra/modules/registry.bicep)
- [infra/modules/foundry.bicep](../infra/modules/foundry.bicep)
- [infra/modules/containerapps.bicep](../infra/modules/containerapps.bicep)

## 2. Prerequisites

Local tooling (Windows):

- Azure CLI: `winget install -e --id Microsoft.AzureCLI`
- Azure Developer CLI: `winget install microsoft.azd`
- Docker Desktop (only required if you build images locally)
- .NET 9 SDK

Azure prerequisites:

- A subscription with permissions to create resource groups and Cognitive Services accounts
- Sufficient quota for the chosen model in the chosen region
- Service principal or user-assigned identity configured for GitHub OIDC if using the pipeline

## 3. One-time setup — GitHub OIDC

The deploy pipeline ([deploy.yml](../.github/workflows/deploy.yml)) uses workload identity federation. Configure once:

1. Create an Entra app registration (or use an existing one):

   ```pwsh
   az ad app create --display-name modernization-assessor-deployer
   ```

2. Create a service principal and capture object ids:

   ```pwsh
   $appId   = az ad app list --display-name modernization-assessor-deployer --query "[0].appId" -o tsv
   $spId    = az ad sp create --id $appId --query id -o tsv
   $tenant  = az account show --query tenantId -o tsv
   $subId   = az account show --query id -o tsv
   ```

3. Assign minimum roles at subscription scope:

   ```pwsh
   az role assignment create --assignee $spId --role "Contributor"          --scope "/subscriptions/$subId"
   az role assignment create --assignee $spId --role "User Access Administrator" --scope "/subscriptions/$subId"
   ```

   The second role is needed because the Bicep deployment creates role assignments.

4. Add a federated credential for the GitHub repo:

   ```pwsh
   az ad app federated-credential create --id $appId --parameters @- <<'JSON'
   {
     "name": "github-modernization-assessor-main",
     "issuer": "https://token.actions.githubusercontent.com",
     "subject": "repo:fabio-padua/modernization-assessor:ref:refs/heads/main",
     "audiences": ["api://AzureADTokenExchange"]
   }
   JSON
   ```

   Add additional federated credentials for environments (for example `environment:prod`) as needed.

5. Add secrets and variables in GitHub:

   Secrets:
   - `AZURE_CLIENT_ID` = `$appId`
   - `AZURE_TENANT_ID` = `$tenant`
   - `AZURE_SUBSCRIPTION_ID` = `$subId`

   Variables (optional, defaults shown):
   - `PARTNER_CODE` = `acme`
   - `ENVIRONMENT_NAME` = `dev`
   - `AZURE_LOCATION` = `northcentralus`
   - `MODEL_DEPLOYMENT_NAME` = `gpt-4.1`
   - `MODEL_NAME` = `gpt-4.1`
   - `MODEL_VERSION` = (leave empty for default)
   - `MODEL_CAPACITY` = `50`

## 4. Pipeline behavior

Trigger: push to `main` (paths under `src/`, `infra/`, or `azure.yaml`) or manual `workflow_dispatch`.

Stages:

1. **provision** — `az deployment sub what-if` and then `az deployment sub create` against `infra/main.bicep`. Outputs are uploaded as an artifact.
2. **build_and_push** — runs `az acr build` against the provisioned ACR using `src/Orchestrator/Dockerfile`. Tags: `<sha>-<run>` and `latest`.
3. **deploy_job** — `az containerapp job update --image <new-image>` on the orchestrator job.
4. **smoke** — `az containerapp job start` to execute a sample assessment (enabled by default; disable via the `run_smoke_assessment` dispatch input).

Concurrency is keyed by branch + environment, so deployments to different environments do not block each other.

## 5. Manual deployment (no CI)

Useful for first-time runs or troubleshooting.

```pwsh
az login
az account set --subscription <subscription-id>

# 1. Provision
az deployment sub create `
  --name modassessor-acme-dev-manual `
  --location northcentralus `
  --template-file infra/main.bicep `
  --parameters `
    partnerCode=acme `
    environmentName=dev `
    location=northcentralus `
    modelDeploymentName=gpt-4.1 `
    modelName=gpt-4.1 `
    modelCapacity=50 `
    operatorPrincipalId=$(az ad signed-in-user show --query id -o tsv)

# 2. Capture outputs
$rg     = "rg-modassessor-acme-dev"
$acr    = az deployment sub show --name modassessor-acme-dev-manual --query "properties.outputs.AZURE_CONTAINER_REGISTRY_NAME.value" -o tsv
$jobName = az deployment sub show --name modassessor-acme-dev-manual --query "properties.outputs.ORCHESTRATOR_JOB_NAME.value" -o tsv

# 3. Build and push image
az acr build `
  --registry $acr `
  --image modernization-assessor/orchestrator:manual `
  --file src/Orchestrator/Dockerfile `
  .

# 4. Update job image
az containerapp job update `
  --name $jobName `
  --resource-group $rg `
  --image "$acr.azurecr.io/modernization-assessor/orchestrator:manual"

# 5. Run a smoke assessment
az containerapp job start --name $jobName --resource-group $rg
```

## 6. Using `azd up`

For developers who prefer the Azure Developer CLI:

```pwsh
azd auth login
azd init
azd env set PARTNER_CODE acme
azd env set ENVIRONMENT_NAME dev
azd up
```

`azure.yaml` declares Bicep as the provider and the orchestrator service. After `azd up`, environment variables such as `AZURE_AI_PROJECT_ENDPOINT` and `ORCHESTRATOR_JOB_NAME` are written into the azd environment and surfaced by the `postprovision` hook.

## 7. Validating a deployment

```pwsh
# Confirm provisioning
az resource list --resource-group rg-modassessor-acme-dev --output table

# Show last execution of the orchestrator job
az containerapp job execution list `
  --name <job-name> --resource-group rg-modassessor-acme-dev `
  --query "[].{name:name, status:properties.status, started:properties.startTime}" `
  --output table

# Stream logs from the most recent execution
az containerapp job logs show `
  --name <job-name> --resource-group rg-modassessor-acme-dev `
  --container orchestrator --follow
```

## 8. Cleanup

```pwsh
az group delete --name rg-modassessor-acme-dev --yes --no-wait
```

Or via azd:

```pwsh
azd down --purge --force
```

`--purge` ensures soft-deleted Cognitive Services (Foundry) resources are also removed, freeing the name for reuse.

## 9. Troubleshooting

- **Quota errors on model deployment** — lower `MODEL_CAPACITY` or change `AZURE_LOCATION`. Use `az cognitiveservices usage list -l <region>` to inspect available quota.
- **Role assignment failures** — confirm the deployer has `User Access Administrator` on the subscription.
- **`acr build` fails** — verify the deployer has AcrPush on the registry (`operatorPrincipalId` parameter handles this automatically).
- **Job stays pending** — check the Container Apps environment quota and the job replica timeout in `infra/modules/containerapps.bicep`.
- **Soft-deleted Foundry name conflict** — purge with `az cognitiveservices account purge --location <region> --resource-group <rg> --name <name>`.

## 10. Promoting to staging or production

1. Create a new GitHub environment (`stg`, `prod`) with its own variables and protection rules.
2. Add a federated credential targeting that environment.
3. Trigger `workflow_dispatch` with `environment_name=stg` (or `prod`) and the desired partner code.

Each environment lands in its own resource group and its own Foundry account, so cross-environment isolation is preserved by default.
