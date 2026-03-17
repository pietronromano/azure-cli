resource "azurerm_virtual_network" "vnet-aca" {
  name                = "vnet-aca"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  address_space       = ["10.0.0.0/16"]
  # dns_servers         = [azurerm_firewall.firewall.ip_configuration.0.private_ip_address] # null
}

resource "azurerm_subnet" "snet-aca" {
  name                 = "snet-aca"
  resource_group_name  = azurerm_virtual_network.vnet-aca.resource_group_name
  virtual_network_name = azurerm_virtual_network.vnet-aca.name
  address_prefixes     = ["10.0.0.0/24"]

  delegation {
    name = "delegation"

    service_delegation {
      name    = "Microsoft.App/environments"
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
    }
  }
}