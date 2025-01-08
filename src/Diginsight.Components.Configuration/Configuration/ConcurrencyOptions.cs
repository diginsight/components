using Diginsight.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diginsight.Components.Configuration;

public class ConcurrencyOptions : IDynamicallyConfigurable, IVolatilelyConfigurable
{
    public int? MaxConcurrency { get; set; }
}

