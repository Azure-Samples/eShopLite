@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param appsvc_outputs_azure_container_registry_endpoint string

param appsvc_outputs_planid string

param appsvc_outputs_azure_container_registry_managed_identity_id string

param appsvc_outputs_azure_container_registry_managed_identity_client_id string

param products_containerimage string

param products_containerport string

param openai_outputs_endpoint string

param products_identity_outputs_id string

param products_identity_outputs_clientid string

resource mainContainer 'Microsoft.Web/sites/sitecontainers@2024-04-01' = {
  name: 'main'
  properties: {
    authType: 'UserAssigned'
    image: products_containerimage
    isMain: true
    userManagedIdentityClientId: appsvc_outputs_azure_container_registry_managed_identity_client_id
  }
  parent: webapp
}

resource webapp 'Microsoft.Web/sites@2024-04-01' = {
  name: take('${toLower('products')}-${uniqueString(resourceGroup().id)}', 60)
  location: location
  properties: {
    serverFarmId: appsvc_outputs_planid
    keyVaultReferenceIdentity: products_identity_outputs_id
    siteConfig: {
      linuxFxVersion: 'SITECONTAINERS'
      acrUseManagedIdentityCreds: true
      acrUserManagedIdentityID: appsvc_outputs_azure_container_registry_managed_identity_client_id
      appSettings: [
        {
          name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EXCEPTION_LOG_ATTRIBUTES'
          value: 'true'
        }
        {
          name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EVENT_LOG_ATTRIBUTES'
          value: 'true'
        }
        {
          name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY'
          value: 'in_memory'
        }
        {
          name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
          value: 'true'
        }
        {
          name: 'HTTP_PORTS'
          value: products_containerport
        }
        {
          name: 'AzureOpenAIEndpoint'
          value: openai_outputs_endpoint
        }
        {
          name: 'AzureOpenAIDeploymentName'
          value: 'gpt-41-mini'
        }
        {
          name: 'AzureOpenAIEmbeddingsDeploymentName'
          value: 'text-embedding-ada-002'
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: products_identity_outputs_clientid
        }
      ]
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${appsvc_outputs_azure_container_registry_managed_identity_id}': { }
      '${products_identity_outputs_id}': { }
    }
  }
}