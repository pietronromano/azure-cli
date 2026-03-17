data "azurerm_monitor_diagnostic_categories" "categories-acr" {
  resource_id = azurerm_container_registry.acr.id
}

resource "azurerm_monitor_diagnostic_setting" "diagnostics_acr" {
  name                           = "diagnostic-settings-acr"
  target_resource_id             = azurerm_container_registry.acr.id
  log_analytics_workspace_id     = azurerm_log_analytics_workspace.workspace.id
  log_analytics_destination_type = "Dedicated" # "AzureDiagnostics"

  dynamic "enabled_log" {
    for_each = data.azurerm_monitor_diagnostic_categories.categories-acr.log_category_types

    content {
      category = enabled_log.key
    }
  }

  dynamic "enabled_metric" {
    for_each = data.azurerm_monitor_diagnostic_categories.categories-acr.metrics

    content {
      category = enabled_metric.key
    }
  }

  lifecycle {
    ignore_changes = [
      log_analytics_destination_type
    ]
  }
}
