using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class TenantConfiguration
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TenantId { get; set; }
        public string ClientTenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
