output "key_vault_id" {
  value = azurerm_key_vault.kv.id
}

output "appreg01_id" {
  value = azuread_application.appreg01.id
}