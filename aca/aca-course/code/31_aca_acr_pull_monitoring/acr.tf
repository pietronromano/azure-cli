# https://github.com/claranet/terraform-azurerm-acr/blob/master/resources.tf

resource "azurerm_container_registry" "acr" {
  name                          = "acr4aca${var.prefix}"
  resource_group_name           = azurerm_resource_group.rg.name
  location                      = azurerm_resource_group.rg.location
  sku                           = "Standard"
  admin_enabled                 = false
  public_network_access_enabled = true
  zone_redundancy_enabled       = false
  anonymous_pull_enabled        = false
  data_endpoint_enabled         = false
  network_rule_bypass_option    = "AzureServices"

  # provisioner "local-exec" {
  #   # interpreter = ["PowerShell", "-Command"]
  #   command = "az acr import --name ${azurerm_container_registry.acr.login_server} --source docker.io/library/hello-world:latest --image hello-world:latest"
  #   when    = create
  # }
}

resource "terraform_data" "acr-import-image" {
  triggers_replace = [
    azurerm_container_registry.acr.id
  ]

  provisioner "local-exec" {
    # interpreter = ["PowerShell", "-Command"]
    command = <<-EOT
      az acr import --name ${azurerm_container_registry.acr.login_server} --source docker.io/library/nginx:1.28 --image nginx:1.28
      # az acr import --name ${azurerm_container_registry.acr.login_server} --source docker.io/library/nginx:1.29 --image nginx:1.29
    EOT
  }
}

# az acr import --name ${azurerm_container_registry.acr.login_server} --source mcr.microsoft.com/mssql/server:2025-latest --image server:2025-latest
