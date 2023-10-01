using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xmasdev2022.DolcettoCarbone.Models;

namespace Xmasdev2022.DolcettoCarbone.Common
{
    
    public class Predictor
    {
        private readonly MLContext _mlContext;

        private ITransformer _model;

        public Predictor()
        {
            _mlContext = new MLContext();
        }
        public ModelOutput Predict(string filePath, ModelInput newSample)
        {
            LoadModel(filePath);

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(_model);
            
            return predictionEngine.Predict(newSample);
        }
        private void LoadModel(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File {filePath} doesn't exist.");
            }

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                _model = _mlContext.Model.Load(stream, out _);
            }

            if (_model == null)
            {
                throw new Exception($"Failed to load Model");
            }
        }
    }
}
