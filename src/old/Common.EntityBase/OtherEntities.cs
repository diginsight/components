using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ObjectProperties
    {
        public ObjectProperties() { this.Properties = new Dictionary<string, object>(); }
        public ObjectProperties(object obj, IDictionary<string, object> properties)
        {
            this.Object = obj;
            this.Properties = properties ?? new Dictionary<string, object>();
        }
        
        public object Object;
        public IDictionary<string, object> Properties;
    }
}
