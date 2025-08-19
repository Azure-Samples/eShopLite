@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource openai 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: take('openai-${uniqueString(resourceGroup().id)}', 64)
  location: location
  kind: 'OpenAI'
  properties: {
    customSubDomainName: toLower(take(concat('openai', uniqueString(resourceGroup().id)), 24))
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: true
  }
  sku: {
    name: 'S0'
  }
  tags: {
    'aspire-resource-name': 'openai'
  }
}

resource gpt_5_mini 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  name: 'gpt-5-mini'
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-5-mini'
      version: '2025-08-07'
    }
  }
  sku: {
    name: 'GlobalStandard'
    capacity: 8
  }
  parent: openai
}

resource text_embedding_ada_002 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  name: 'text-embedding-ada-002'
  properties: {
    model: {
      format: 'OpenAI'
      name: 'text-embedding-ada-002'
      version: '1'
    }
  }
  sku: {
    name: 'Standard'
    capacity: 8
  }
  parent: openai
  dependsOn: [
    gpt_5_mini
  ]
}

output connectionString string = 'Endpoint=${openai.properties.endpoint}'

output name string = openai.name