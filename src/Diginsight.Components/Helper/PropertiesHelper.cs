﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diginsight.Components;

public static class PROPS
{
    //public static Dictionary<string, object> GetD(Dictionary<string, object> props) { return props; }

    public static Dictionary<string, object> Get((string, object)[] props) { return props.ToDictionary(t => t.Item1, t => t.Item2); }

}
