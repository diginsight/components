variable "additional_kv_secret_readers" {
  type    = set(string)
  default = []
}

variable "additional_kv_secret_writers" {
  type    = set(string)
  default = []
}