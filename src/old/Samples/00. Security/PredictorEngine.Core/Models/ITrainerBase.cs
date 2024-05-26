using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xmasdev2022.DolcettoCarbone.Models
{
    public interface ITrainerBase
    {
        string Name { get; }
        void Fit(string trainingFileName);
        BinaryClassificationMetrics Evaluate();
        void Save(string path);
    }
}
