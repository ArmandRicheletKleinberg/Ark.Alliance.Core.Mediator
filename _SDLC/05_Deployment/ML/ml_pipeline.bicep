param location string = resourceGroup().location
param enableMl bool = true
param sku string = 'Standard'

resource ehNamespace 'Microsoft.EventHub/namespaces@2023-01-01-preview' = if (enableMl) {
  name: 'arkml-${uniqueString(resourceGroup().id)}'
  location: location
  sku: {
    name: sku
    tier: sku
  }
}

resource eventHub 'Microsoft.EventHub/namespaces/eventhubs@2023-01-01-preview' = if (enableMl) {
  parent: ehNamespace
  name: 'telemetry'
}

resource storage 'Microsoft.Storage/storageAccounts@2022-09-01' = if (enableMl) {
  name: 'arkmldatalake${uniqueString(resourceGroup().id)}'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    isHnsEnabled: true
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = if (enableMl) {
  name: 'arkmlkv${uniqueString(resourceGroup().id)}'
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableSoftDelete: true
  }
}
