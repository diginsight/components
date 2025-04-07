terraform {
  required_version = "~> 1.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = "tfstate"
    storage_account_name = "tfstatel590c"
    container_name       = "tfstate"
    key                  = "authsample.tfstate"
    use_cli              = true
    subscription_id      = "ad8e9db6-430d-46a8-988d-27fcec619def"
  }
}

check "env_name" {
  assert {
    condition     = contains(["testms", "stagems", "prodms"], local.env_name)
    error_message = "`terraform.workspace` must be 'testms', 'stagems' or 'prodms'"
  }
}

locals {
  rg_name  = "authsample-${local.env_name}-rg-01"
}

resource "random_string" "password" {
  length  = 16
  special = true
}
resource "random_string" "name" {
  length  = 16
  special = false
}
resource "random_string" "location" {
  length  = 16
  special = false
}

resource "azuread_group" "contributors" {
  display_name     = "authsample-contributors-${local.env_name}-01"
  mail_enabled     = false
  security_enabled = true
}

resource "azuread_group_member" "contributors" {
  for_each = toset(data.azuread_users.contributors.object_ids)

  group_object_id  = azuread_group.contributors.object_id
  member_object_id = each.value

  depends_on = [azuread_group.contributors]
}

resource "azuread_application" "appreg01" {
  display_name = "authsample-app-${local.env_name}-01"

  owners = data.azuread_users.owners.object_ids

  api {
    oauth2_permission_scope {
      admin_consent_description  = "access_as_user - description"
      admin_consent_display_name = "access_as_user - display name"
      enabled                    = true
      id                         = uuid()
      type                       = "User"
      value                      = "access_as_user"
    }
  }
}

resource "azuread_application" "appreg02" {
  display_name = "authsample-app-${local.env_name}-02"

  owners = data.azuread_users.owners.object_ids

  api {
    oauth2_permission_scope {
      admin_consent_description  = "access_as_user - description"
      admin_consent_display_name = "access_as_user - display name"
      enabled                    = true
      id                         = uuid()
      type                       = "User"
      value                      = "access_as_user"
    }
  }
}

resource "azurerm_key_vault_access_policy" "additional_readers_appreg01" {
  key_vault_id = azurerm_key_vault.kv.id

  tenant_id = local.tenant_id
  object_id = azuread_application.appreg01.object_id

  secret_permissions = ["Get", "List"]
}

resource "azurerm_key_vault_access_policy" "additional_readers_appreg02" {
  key_vault_id = azurerm_key_vault.kv.id

  tenant_id = local.tenant_id
  object_id = azuread_application.appreg02.object_id

  secret_permissions = ["Get", "List"]
}

resource "azuread_application_identifier_uri" "appreg01uri" {
  application_id = azuread_application.appreg01.id

  identifier_uri = "api://${azuread_application.appreg01.client_id}"
}

resource "azuread_application_identifier_uri" "appreg02uri" {
  application_id = azuread_application.appreg02.id

  identifier_uri = "api://${azuread_application.appreg02.client_id}"
}

resource "azuread_application_password" "appreg01secret" {
  application_id = azuread_application.appreg01.id
  display_name   = "authsample-app-${local.env_name}-01-secret"

  end_date = "2299-12-31T23:59:59Z"
}

resource "azuread_application_password" "appreg02secret" {
  application_id = azuread_application.appreg02.id
  display_name   = "authsample-app-${local.env_name}-02-secret"

  end_date = "2299-12-31T23:59:59Z"
}

resource "azuread_application_pre_authorized" "azcli01" {
  application_id       = azuread_application.appreg01.id
  authorized_client_id = data.azuread_application_published_app_ids.well_known.result["MicrosoftAzureCli"]

  permission_ids = flatten([
    for api_block in azuread_application.appreg01.api : [
      for scope in api_block.oauth2_permission_scope : scope.id
    ]
  ])

   depends_on = [azuread_application.appreg01]
}

resource "azuread_application_pre_authorized" "azcli02" {
  application_id       = azuread_application.appreg02.id
  authorized_client_id = data.azuread_application_published_app_ids.well_known.result["MicrosoftAzureCli"]

  permission_ids = flatten([
    for api_block in azuread_application.appreg02.api : [
      for scope in api_block.oauth2_permission_scope : scope.id
    ]
  ])

   depends_on = [azuread_application.appreg02]
}


resource "azurerm_resource_group" "rg" {
  name     = "${local.rg_name}"
  location = "${local.location}"
}

resource "azurerm_key_vault" "kv" {
  name                = "authsample-kv-${local.env_name}-01"
  resource_group_name = local.rg_name
  location            = local.location
  tenant_id           = local.tenant_id

  sku_name = "standard"
}

resource "azurerm_key_vault_access_policy" "contributors" {
  key_vault_id = azurerm_key_vault.kv.id

  tenant_id = local.tenant_id
  object_id = azuread_group.contributors.object_id

  key_permissions         = ["Backup", "Create", "Delete", "Get", "Import", "List", "Purge", "Recover", "Restore", "Update", "Release", "Rotate", "GetRotationPolicy", "SetRotationPolicy"]
  secret_permissions      = ["Backup", "Delete", "Get", "List", "Purge", "Recover", "Restore", "Set"]
  certificate_permissions = ["Backup", "Create", "Delete", "DeleteIssuers", "Get", "GetIssuers", "Import", "List", "ListIssuers", "ManageContacts", "ManageIssuers", "Purge", "Recover", "Restore", "SetIssuers", "Update"]

  depends_on = [azuread_group.contributors]
}

resource "azurerm_key_vault_secret" "AuthenticationSampleApi_ClientSecret" {
  name         = "AuthenticationSampleApi--ClientSecret"
  value        = azuread_application_password.appreg01secret.value
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [azurerm_key_vault.kv]
}

resource "azurerm_key_vault_secret" "AzureAD_ClientSecret" {
  name         = "AzureAD--ClientSecret"
  value        = azuread_application_password.appreg01secret.value
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [azurerm_key_vault.kv]
}

resource "azurerm_key_vault_secret" "AuthenticationSampleServerApi_ClientSecret" {
  name         = "AuthenticationSampleServerApi--ClientSecret"
  value        = azuread_application_password.appreg02secret.value
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [azurerm_key_vault.kv]
}

resource "azurerm_key_vault_secret" "AuthenticationSampleServerApiDelegated_ClientSecret" {
  name         = "AuthenticationSampleServerApiDelegated--ClientSecret"
  value        = azuread_application_password.appreg02secret.value
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [azurerm_key_vault.kv]
}

resource "azurerm_key_vault_secret" "AzureAD02_ClientSecret" {
  name         = "AzureAD02--ClientSecret"
  value        = azuread_application_password.appreg01secret.value
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [azurerm_key_vault.kv]
}

resource "azurerm_log_analytics_workspace" "law" {
  name                = "authsample-law-${local.env_name}-01"
  resource_group_name = local.rg_name
  location            = local.location

  retention_in_days = 30
}

resource "azurerm_application_insights" "ain" {
  name                = "authsample-ain-${local.env_name}-01"
  resource_group_name = local.rg_name
  location            = local.location

  workspace_id     = azurerm_log_analytics_workspace.law.id
  application_type = "web"
}

resource "azurerm_service_plan" "plan" {
  location            = "${local.location}"
  name                = "authsample-plan-${local.env_name}-01"
  os_type             = "Windows"
  resource_group_name = "${local.rg_name}"
  sku_name            = "B1"
  depends_on = [
    azurerm_resource_group.rg,
  ]
}

resource "azurerm_windows_web_app" "api" {
  app_settings = {
    APPINSIGHTS_INSTRUMENTATIONKEY                             = azurerm_application_insights.ain.instrumentation_key
    APPINSIGHTS_PROFILERFEATURE_VERSION                        = "1.0.0"
    APPINSIGHTS_SNAPSHOTFEATURE_VERSION                        = "1.0.0"
    APPLICATIONINSIGHTS_CONNECTION_STRING                      = azurerm_application_insights.ain.connection_string
    ASPNETCORE_ENVIRONMENT                                     = "Production"
    ApplicationInsightsAgent_EXTENSION_VERSION                 = "~2"
    AppsettingsEnvironmentName                                 = "${local.env_name}"
    DiagnosticServices_EXTENSION_VERSION                       = "~3"
    InstrumentationEngine_EXTENSION_VERSION                    = "~1"
    IsSwaggerEnabled                                           = "true"
    SnapshotDebugger_EXTENSION_VERSION                         = "disabled"
    XDT_MicrosoftApplicationInsights_BaseExtensions            = "~1"
    XDT_MicrosoftApplicationInsights_Java                      = "1"
    XDT_MicrosoftApplicationInsights_Mode                      = "recommended"
    XDT_MicrosoftApplicationInsights_NodeJS                    = "1"
    XDT_MicrosoftApplicationInsights_PreemptSdk                = "1"
  }
  https_only          = true
  location            = "${local.location}"
  name                = "authsample-api-${local.env_name}-01"
  resource_group_name = "${local.rg_name}"
  service_plan_id     = "/subscriptions/${local.subscription_id}/resourceGroups/${local.rg_name}/providers/Microsoft.Web/serverFarms/authsample-plan-${local.env_name}-01"
  tags = {
    "hidden-link: /app-insights-conn-string"         = azurerm_application_insights.ain.connection_string
    "hidden-link: /app-insights-instrumentation-key" = azurerm_application_insights.ain.instrumentation_key
    "hidden-link: /app-insights-resource-id"         = azurerm_application_insights.ain.id
  }
  webdeploy_publish_basic_authentication_enabled = false
  identity {
    type = "SystemAssigned"
  }
  site_config {
    ftps_state                        = "FtpsOnly"
    # ip_restriction_default_action     = ""
    # scm_ip_restriction_default_action = ""
    use_32_bit_worker                 = false
  }
  sticky_settings {
    app_setting_names = ["APPINSIGHTS_INSTRUMENTATIONKEY", "APPINSIGHTS_PROFILERFEATURE_VERSION", "APPINSIGHTS_SNAPSHOTFEATURE_VERSION", "ApplicationInsightsAgent_EXTENSION_VERSION", "DiagnosticServices_EXTENSION_VERSION", "InstrumentationEngine_EXTENSION_VERSION", "SnapshotDebugger_EXTENSION_VERSION", "XDT_MicrosoftApplicationInsights_BaseExtensions", "XDT_MicrosoftApplicationInsights_Mode", "XDT_MicrosoftApplicationInsights_NodeJS", "XDT_MicrosoftApplicationInsights_PreemptSdk", "APPLICATIONINSIGHTS_CONNECTION_STRING ", "APPLICATIONINSIGHTS_CONFIGURATION_CONTENT", "XDT_MicrosoftApplicationInsightsJava"]
  }
  depends_on = [
    azurerm_service_plan.plan
  ]
}

resource "azurerm_application_insights" "ain2" {
  name                = "authsample-ain-${local.env_name}-02"
  resource_group_name = local.rg_name
  location            = local.location

  workspace_id     = azurerm_log_analytics_workspace.law.id
  application_type = "web"
}

resource "azurerm_windows_web_app" "api2" {
  app_settings = {
    APPINSIGHTS_INSTRUMENTATIONKEY                             = azurerm_application_insights.ain2.instrumentation_key
    APPINSIGHTS_PROFILERFEATURE_VERSION                        = "1.0.0"
    APPINSIGHTS_SNAPSHOTFEATURE_VERSION                        = "1.0.0"
    APPLICATIONINSIGHTS_CONNECTION_STRING                      = azurerm_application_insights.ain2.connection_string
    ASPNETCORE_ENVIRONMENT                                     = "Production"
    ApplicationInsightsAgent_EXTENSION_VERSION                 = "~2"
    AppsettingsEnvironmentName                                 = "${local.env_name}"
    DiagnosticServices_EXTENSION_VERSION                       = "~3"
    InstrumentationEngine_EXTENSION_VERSION                    = "~1"
    IsSwaggerEnabled                                           = "true"
    SnapshotDebugger_EXTENSION_VERSION                         = "disabled"
    XDT_MicrosoftApplicationInsights_BaseExtensions            = "~1"
    XDT_MicrosoftApplicationInsights_Java                      = "1"
    XDT_MicrosoftApplicationInsights_Mode                      = "recommended"
    XDT_MicrosoftApplicationInsights_NodeJS                    = "1"
    XDT_MicrosoftApplicationInsights_PreemptSdk                = "1"
  }
  https_only          = true
  location            = "${local.location}"
  name                = "authsample-api-${local.env_name}-02"
  resource_group_name = "${local.rg_name}"
  service_plan_id     = "/subscriptions/${local.subscription_id}/resourceGroups/${local.rg_name}/providers/Microsoft.Web/serverFarms/authsample-plan-${local.env_name}-01"
  tags = {
    "hidden-link: /app-insights-conn-string"         = azurerm_application_insights.ain2.connection_string
    "hidden-link: /app-insights-instrumentation-key" = azurerm_application_insights.ain2.instrumentation_key
    "hidden-link: /app-insights-resource-id"         = azurerm_application_insights.ain2.id
  }
  webdeploy_publish_basic_authentication_enabled = false
  identity {
    type = "SystemAssigned"
  }
  site_config {
    ftps_state                        = "FtpsOnly"
    # ip_restriction_default_action     = ""
    # scm_ip_restriction_default_action = ""
    use_32_bit_worker                 = false
  }
  sticky_settings {
    app_setting_names = ["APPINSIGHTS_INSTRUMENTATIONKEY", "APPINSIGHTS_PROFILERFEATURE_VERSION", "APPINSIGHTS_SNAPSHOTFEATURE_VERSION", "ApplicationInsightsAgent_EXTENSION_VERSION", "DiagnosticServices_EXTENSION_VERSION", "InstrumentationEngine_EXTENSION_VERSION", "SnapshotDebugger_EXTENSION_VERSION", "XDT_MicrosoftApplicationInsights_BaseExtensions", "XDT_MicrosoftApplicationInsights_Mode", "XDT_MicrosoftApplicationInsights_NodeJS", "XDT_MicrosoftApplicationInsights_PreemptSdk", "APPLICATIONINSIGHTS_CONNECTION_STRING ", "APPLICATIONINSIGHTS_CONFIGURATION_CONTENT", "XDT_MicrosoftApplicationInsightsJava"]
  }
  depends_on = [
    azurerm_service_plan.plan
  ]
}

resource "azurerm_key_vault_access_policy" "additional_readers" {
  for_each = var.additional_kv_secret_readers
  

  key_vault_id = azurerm_key_vault.kv.id

  tenant_id = local.tenant_id
  object_id = each.key

  secret_permissions = ["Get", "List"]

  depends_on = [azurerm_key_vault.kv]
}


resource "azurerm_key_vault_access_policy" "additional_writers" {
  for_each = var.additional_kv_secret_writers

  key_vault_id = azurerm_key_vault.kv.id

  tenant_id = local.tenant_id
  object_id = each.key

  secret_permissions = ["Delete", "Get", "List", "Purge", "Set"]

  depends_on = [azurerm_key_vault.kv]
}
