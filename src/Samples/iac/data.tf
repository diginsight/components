data "azuread_users" "owners" {
  user_principal_names = [
    "dario.airoldi_live.com#EXT#@famigliaairoldi.onmicrosoft.com"
  ]
}

data "azuread_users" "contributors" {
  user_principal_names = [
    "dario.airoldi_live.com#EXT#@famigliaairoldi.onmicrosoft.com"
  ]
}

data "azurerm_client_config" "current" {}

data "azuread_application_published_app_ids" "well_known" {}