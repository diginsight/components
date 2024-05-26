using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xmasdev2022.DolcettoCarbone.Models
{
    public class ModelOutput
    {
        public bool PredictedLabel { get; set; }
        public float Score { get; set; }

        public float Probability { get; set; }
    }
}
