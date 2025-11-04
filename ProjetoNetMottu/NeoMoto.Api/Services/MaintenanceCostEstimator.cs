using Microsoft.ML;

namespace NeoMoto.Api.Services;

public class MaintenanceFeatures
{
    public float AgeYears { get; set; }
    public float DaysSinceLastService { get; set; }
    public float ServiceType { get; set; }
}

public class MaintenancePrediction
{
    public float Score { get; set; }
}

public interface IMaintenanceCostEstimator
{
    float Predict(MaintenanceFeatures features);
}

public class MaintenanceCostEstimator : IMaintenanceCostEstimator, IDisposable
{
    private readonly MLContext _ml;
    private readonly ITransformer _model;
    private readonly PredictionEngine<MaintenanceFeatures, MaintenancePrediction> _engine;

    public MaintenanceCostEstimator()
    {
        _ml = new MLContext(seed: 123);

        var trainingData = GenerateSyntheticData();

        var dataView = _ml.Data.LoadFromEnumerable(trainingData);

        var pipeline = _ml.Transforms.Concatenate("Features", nameof(MaintenanceFeatures.AgeYears), nameof(MaintenanceFeatures.DaysSinceLastService), nameof(MaintenanceFeatures.ServiceType))
            .Append(_ml.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features"));

        _model = pipeline.Fit(dataView);
        _engine = _ml.Model.CreatePredictionEngine<MaintenanceFeatures, MaintenancePrediction>(_model);
    }

    public float Predict(MaintenanceFeatures features)
    {
        var p = _engine.Predict(features);
        return MathF.Max(0f, p.Score);
    }

    private static IEnumerable<ModelRow> GenerateSyntheticData()
    {
        // Label = 50 + 10*Age + 0.5*Days + ServiceTypeFactor
        // ServiceType: 0=inspeção(0), 1=normal(40), 2=pesada(100)
        var serviceFactors = new Dictionary<int, float> { { 0, 0f }, { 1, 40f }, { 2, 100f } };
        for (int age = 0; age <= 10; age++)
        {
            for (int days = 0; days <= 200; days += 20)
            {
                foreach (var st in serviceFactors.Keys)
                {
                    var label = 50f + 10f * age + 0.5f * days + serviceFactors[st];
                    yield return new ModelRow
                    {
                        AgeYears = age,
                        DaysSinceLastService = days,
                        ServiceType = st,
                        Label = label
                    };
                }
            }
        }
    }

    private class ModelRow
    {
        public float AgeYears { get; set; }
        public float DaysSinceLastService { get; set; }
        public float ServiceType { get; set; }
        public float Label { get; set; }
    }

    public void Dispose()
    {
        _engine.Dispose();
    }
}
