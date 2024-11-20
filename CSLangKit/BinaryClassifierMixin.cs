namespace CSLangKit;

public abstract class BinaryClassifierBaseMixin
{
    public abstract List<ClassificationExample> Transform(List<ClassificationExample> examples);

    public Dictionary<string, double> Score(List<ClassificationExample> examples)
    {
        double truePositives = 0;
        double trueNegatives = 0;
        double falsePositives = 0;
        double falseNegatives = 0;

        Dictionary<string, double> metrics = new Dictionary<string, double>();

        var predictions = this.Transform(examples);
        var trueAndPredictedLabelPairs = examples.Zip(
            predictions,
            (yTrue, yPred) => (yTrue.Label, yPred.Label)
        );

        foreach (var (TrueLabel, PredictedLabel) in trueAndPredictedLabelPairs)
        {
            if (TrueLabel == 1 && PredictedLabel == 1)
            {
                truePositives++;
            }
            if (TrueLabel == 0 && PredictedLabel == 0)
            {
                trueNegatives++;
            }
            if (TrueLabel == 0 && PredictedLabel == 1)
            {
                falsePositives++;
            }
            if (TrueLabel == 1 && PredictedLabel == 0)
            {
                falseNegatives++;
            }
        }

        double positives = falseNegatives + truePositives;
        double negatives = falsePositives + trueNegatives;

        // see wikipedia for formula definitions
        metrics["accuracy"] = (truePositives + trueNegatives) / (double)examples.Count;
        metrics["recall"] = truePositives / (truePositives + falseNegatives);
        metrics["precision"] = truePositives / (truePositives + falsePositives);
        metrics["F1Score"] =
            2.0 / (Math.Pow(metrics["precision"], -1.0) + Math.Pow(metrics["recall"], -1.0));

        return metrics;
    }
}
