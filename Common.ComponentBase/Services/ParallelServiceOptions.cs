using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public sealed class ParallelServiceOptions : IParallelServiceOptions
    {
        public int LowConcurrency { get; set; }
        public int MediumConcurrency { get; set; }
        public int HighConcurrency { get; set; }
    }
}
