resource "azurerm_resource_group" "rg" {
  name     = "rg-frontdoor-aca-${var.prefix}"
  location = "swedencentral"
}