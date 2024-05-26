
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public sealed class KeyVaultTokenInfo
    {
        public string access_token { get; set; }
        public string client_id { get; set; }
        public string expires_in { get; set; }
        public string expires_on { get; set; }
        public string ext_expires_in { get; set; }
        public string not_before { get; set; }
        public string resource { get; set; }
        public string token_type { get; set; }
    }

    public sealed class KeyVaultSecretInfo
    {
        public string value { get; set; }
        public string id { get; set; }
        //public Dictionary<string, string> attributes { get; set; }
    }
}
