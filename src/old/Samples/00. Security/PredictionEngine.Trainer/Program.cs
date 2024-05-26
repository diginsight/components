using System.Xml.Linq;
using Xmasdev2022.DolcettoCarbone.Common;
using Xmasdev2022.DolcettoCarbone.Models;
using Xmasdev2022.DolcettoCarbone.Trainers;

var trainers = new List<ITrainerBase>
            {
                new SdcaLogisticRegressionTrainer(),
                new LbfgsLogisticRegressionTrainer(),
                //new AveragedPerceptronTrainer(),
                //new PriorTrainer(),
                //new SdcaNonCalibratedTrainer(),
                //new SgdCalibratedTrainer(),
                //new SgdNonCalibratedTrainer()
            };

var newSample = new ModelInput()
{
    GiocattoliRotti = 12,
    MediaVoti = 4,
    Note = 1,
    Parolacce = 2,
    VisiteNonni = 122
};

trainers.ForEach(t => TrainEvaluatePredict(t, newSample));

static void TrainEvaluatePredict(ITrainerBase trainer, ModelInput newSample)
{
    Console.WriteLine("*******************************");
    Console.WriteLine($"{trainer.Name}");
    Console.WriteLine("*******************************");

    string basePath = $"{Directory.GetCurrentDirectory()}\\Data";
    string path = $"{basePath}\\LogSequence.csv";
    
    // Fit with training data
    trainer.Fit(path);

    //Evaluate
    var modelMetrics = trainer.Evaluate();
    Console.WriteLine($"Accuracy: {modelMetrics.Accuracy:0.##}{Environment.NewLine}" +
                      $"Positive Precision: {modelMetrics.PositivePrecision:#.##}{Environment.NewLine}" +
                      $"Positive Recall: {modelMetrics.PositiveRecall:#.##}{Environment.NewLine}" +
                      $"F1 Score: {modelMetrics.F1Score:#.##}{Environment.NewLine}" +
                      $"Area Under Roc Curve: {modelMetrics.AreaUnderRocCurve:#.##}{Environment.NewLine}");

    Console.WriteLine($"{modelMetrics.ConfusionMatrix.GetFormattedConfusionTable()}");

    trainer.Save(basePath);

    // usi il modello nell'applicazione
    var predictor = new Predictor();
    string filePath = $"{basePath}\\classification.mdl";
    var prediction = predictor.Predict(filePath, newSample);
    Console.WriteLine("------------------------------");
    Console.WriteLine($"Prediction: {prediction.PredictedLabel:#.##}");
    Console.WriteLine("------------------------------");
}

