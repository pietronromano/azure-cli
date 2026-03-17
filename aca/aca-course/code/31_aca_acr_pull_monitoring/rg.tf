resource "azurerm_resource_group" "rg" {
  name     = "rg-aca-acr-monitoring-${var.prefix}"
  location = "swedencentral"
}