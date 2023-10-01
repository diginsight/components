using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xmasdev2022.DolcettoCarbone.Models
{
    public class ModelInput
    {
        [LoadColumn(0)]
        public float Note { get; set; }

        [LoadColumn(1)]
        public float GiocattoliRotti { get; set; }

        [LoadColumn(2)]
        public float Parolacce { get; set; }

        [LoadColumn(3)]
        public float VisiteNonni { get; set; }

        [LoadColumn(4)]
        public float MediaVoti { get; set; }

        [LoadColumn(5)]
        public bool Label { get; set; }
    }
}
