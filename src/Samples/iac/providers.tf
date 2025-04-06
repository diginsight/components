provider "azurerm" {
  features {}

  use_cli = true
  tenant_id       = local.tenant_id
  subscription_id = local.subscription_id
}

provider "azuread" {
  use_cli = true
  tenant_id = local.tenant_id
}

provider "random" {}