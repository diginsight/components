using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xmasdev2022.DolcettoCarbone.Models;

namespace Xmasdev2022.DolcettoCarbone.Common
{
    public class Trainer<TParameters> : ITrainerBase
                where TParameters : class
    {
        protected readonly MLContext MlContext;
        protected DataOperationsCatalog.TrainTestData _dataSplit;

        protected ITransformer _trainedModel;
        protected ITrainerEstimator<BinaryPredictionTransformer<TParameters>, TParameters> _model;

        public string Name { get; protected set; }

        public Trainer()
        {
            MlContext = new MLContext();
        }

        //Fit with training data
        public void Fit(string trainingFileName)
        {
            if (!File.Exists(trainingFileName))
            {
                throw new FileNotFoundException($"File {trainingFileName} doesn't exist.");
            }

            _dataSplit = LoadAndPrepareData(trainingFileName);

            //build Data Processing Pipeline
            var dataProcessPipeline = BuildDataProcessingPipeline();

            //append Trainer to the pipeline
            var trainingPipeline = dataProcessPipeline.Append(_model);

            //train model with TrainSet (70%) - data still in memory
            _trainedModel = trainingPipeline.Fit(_dataSplit.TrainSet);

            //debug it to see normalized trainset
            var dataDebuggerPreview = _trainedModel.Transform(_dataSplit.TrainSet).Preview();
        }
        private DataOperationsCatalog.TrainTestData LoadAndPrepareData(string trainingFileName)
        {
            var trainingDataView = MlContext.Data
                                    .LoadFromTextFile<ModelInput>
                                      (trainingFileName, hasHeader: true, separatorChar: ';');

            //splitta il dataset in due parti: 70% training, 30% test che servirà per la valutazione del modello
            return MlContext.Data.TrainTestSplit(trainingDataView, testFraction: 0.3);
        }

        private EstimatorChain<NormalizingTransformer> BuildDataProcessingPipeline()
        {
            //concatena le feature interessanti per il modello
            var dataProcessPipeline = MlContext.Transforms.Concatenate("Features",
                                               nameof(ModelInput.Note),
                                               nameof(ModelInput.GiocattoliRotti),
                                               nameof(ModelInput.Parolacce),
                                               nameof(ModelInput.VisiteNonni),
                                               nameof(ModelInput.MediaVoti)
                                               )

               //esegue la normalizzazione dei dati, per portare i dati a una proporzione comune,
               //visto che alcuni algoritmi sono più sensibili di altri

               //come capisco se il trainer necessita normalizzazione? è descritto nella documentazione
               //https://learn.microsoft.com/en-us/dotnet/api/microsoft.ml.trainers.sdcalogisticregressionbinarytrainer?view=ml-dotnet
               //https://learn.microsoft.com/en-us/dotnet/machine-learning/resources/tasks

               .Append(MlContext.Transforms.NormalizeMinMax("NormalizedFeatures", "Features"))
               .AppendCacheCheckpoint(MlContext);

            return dataProcessPipeline;
        }

        public BinaryClassificationMetrics Evaluate()
        {
            var testSetTransform = _trainedModel.Transform(_dataSplit.TestSet);
            var tested = testSetTransform.Preview();

            return MlContext.BinaryClassification.EvaluateNonCalibrated(testSetTransform);
        }

        public void Save(string path)
        {
            var filePath = Path.Combine(path, "classification.mdl");
            MlContext.Model.Save(_trainedModel, _dataSplit.TrainSet.Schema, filePath);
        }
    }
}
