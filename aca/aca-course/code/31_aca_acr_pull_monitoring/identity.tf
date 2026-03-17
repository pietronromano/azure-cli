resource "azurerm_user_assigned_identity" "identity-aca" {
  name                = "identity-aca"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
}

resource "azurerm_role_assignment" "acr-pull" {
  role_definition_name = "ACRPull"
  principal_id         = azurerm_user_assigned_identity.identity-aca.principal_id
  scope                = azurerm_container_registry.acr.id
}

output "identity_principal_id" {
  value = azurerm_user_assigned_identity.identity-aca.principal_id
}