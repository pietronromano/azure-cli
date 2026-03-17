resource "azurerm_container_app" "aca-nginx" {
  name                         = "nginx"
  container_app_environment_id = azurerm_container_app_environment.env.id
  resource_group_name          = azurerm_resource_group.rg.name
  revision_mode                = "Single"
  workload_profile_name        = "profile-D4" # "Consumption"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.identity-aca.id]
  }

  template {
    min_replicas = 15
    max_replicas = 15

    container {
      name   = "nginx-129"
      image  = "acr4aca31.azurecr.io/nginx:1.29"
      cpu    = 0.25
      memory = "0.5Gi"
    }
  }

  ingress {
    allow_insecure_connections = false
    external_enabled           = true
    target_port                = 80
    transport                  = "auto"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  registry {
    server   = azurerm_container_registry.acr.login_server
    identity = azurerm_user_assigned_identity.identity-aca.id
  }
}

# output "aca_inspector_gadget_fqdn" {
#   value = azurerm_container_app.aca-inspector-gadget.ingress.0.fqdn
# }

output "aca_nginx_fqdn" {
  value = azurerm_container_app.aca-nginx.ingress.0.fqdn
}

output "aca_outbound_ips" {
  value = azurerm_container_app.aca-nginx.outbound_ip_addresses
}