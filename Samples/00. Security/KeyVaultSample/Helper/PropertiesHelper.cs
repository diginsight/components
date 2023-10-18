using Azure.Core;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.NativeInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyVaultSample
{
    internal static class PROPS
    {
        public static Dictionary<string, object> Get((string, object)[] props) { return props.ToDictionary(t => t.Item1, t => t.Item2); }
    }

}