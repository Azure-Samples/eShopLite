targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention, the name of the resource group for your application will use this name, prefixed with rg-')
param environmentName string

@minLength(1)
@description('The location used for all deployed resources')
param location string

@description('Id of the user or app to assign application roles')
param principalId string = ''


var tags = {
  'azd-env-name': environmentName
}

resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module appsvc 'appsvc/appsvc.module.bicep' = {
  name: 'appsvc'
  scope: rg
  params: {
    location: location
    userPrincipalId: principalId
  }
}
module openai 'openai/openai.module.bicep' = {
  name: 'openai'
  scope: rg
  params: {
    location: location
  }
}
module products_identity 'products-identity/products-identity.module.bicep' = {
  name: 'products-identity'
  scope: rg
  params: {
    location: location
  }
}
module products_roles_openai 'products-roles-openai/products-roles-openai.module.bicep' = {
  name: 'products-roles-openai'
  scope: rg
  params: {
    location: location
    openai_outputs_name: openai.outputs.name
    principalId: products_identity.outputs.principalId
  }
}
output APPSVC_AZURE_CONTAINER_REGISTRY_ENDPOINT string = appsvc.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
output APPSVC_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_CLIENT_ID string = appsvc.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_CLIENT_ID
output APPSVC_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = appsvc.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID
output APPSVC_PLANID string = appsvc.outputs.planId
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = appsvc.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
output OPENAI_CONNECTIONSTRING string = openai.outputs.connectionString
output OPENAI_ENDPOINT string = openai.outputs.endpoint
output PRODUCTS_IDENTITY_CLIENTID string = products_identity.outputs.clientId
output PRODUCTS_IDENTITY_ID string = products_identity.outputs.id
