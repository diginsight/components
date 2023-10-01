using Microsoft.ML;
using Microsoft.ML.Calibrators;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xmasdev2022.DolcettoCarbone.Common;
using Xmasdev2022.DolcettoCarbone.Models;


namespace Xmasdev2022.DolcettoCarbone.Trainers
{
    public class LbfgsLogisticRegressionTrainer :
    Trainer<CalibratedModelParametersBase<LinearBinaryModelParameters,
                                                        PlattCalibrator>>
    {
        public LbfgsLogisticRegressionTrainer() : base()
        {
            Name = "LBFGS Logistic Regression";
            _model = MlContext
        .BinaryClassification
        .Trainers
        .LbfgsLogisticRegression(labelColumnName: "Label", featureColumnName: "NormalizedFeatures");
        }
    }

    public class AveragedPerceptronTrainer :
        Trainer<LinearBinaryModelParameters>
    {
        public AveragedPerceptronTrainer() : base()
        {
            Name = "Averaged Perceptron";
            _model = MlContext
        .BinaryClassification
        .Trainers
        .AveragedPerceptron(labelColumnName: "Label", featureColumnName: "Features");
        }
    }

    public class PriorTrainer :
        Trainer<PriorModelParameters>
    {
        public PriorTrainer() : base()
        {
            Name = "Prior";
            _model = MlContext
        .BinaryClassification
        .Trainers
        .Prior(labelColumnName: "Label");
        }
    }

    public class SdcaLogisticRegressionTrainer :
        Trainer<CalibratedModelParametersBase<LinearBinaryModelParameters,
                                              PlattCalibrator>>
    {
        public SdcaLogisticRegressionTrainer() : base()
        {
            Name = "Sdca Logistic Regression";
            _model = MlContext
        .BinaryClassification
        .Trainers
        .SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "NormalizedFeatures");
        }
    }

    public class SdcaNonCalibratedTrainer :
        Trainer<LinearBinaryModelParameters>
    {
        public SdcaNonCalibratedTrainer() : base()
        {
            Name = "Sdca NonCalibrated";
            _model = MlContext
        .BinaryClassification
        .Trainers
        .SdcaNonCalibrated(labelColumnName: "Label", featureColumnName: "Features");
        }
    }

    public class SgdCalibratedTrainer
        : Trainer<CalibratedModelParametersBase<LinearBinaryModelParameters, PlattCalibrator>>
    {
        public SgdCalibratedTrainer() : base()
        {
            Name = "Sgd Calibrated";
            _model = MlContext
            .BinaryClassification
            .Trainers
            .SgdCalibrated(labelColumnName: "Label", featureColumnName: "Features");
        }
    }

    public class SgdNonCalibratedTrainer : Trainer<LinearBinaryModelParameters>
    {
        public SgdNonCalibratedTrainer() : base()
        {
            Name = "Sgd NonCalibrated";
            _model = MlContext
            .BinaryClassification
            .Trainers
            .SgdNonCalibrated(labelColumnName: "Label", featureColumnName: "Features");
        }
    }
}
