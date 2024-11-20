namespace CSLangKit;

public class PerceptronClassifer : BinaryClassifierBaseMixin, IModel<ClassificationExample>
{
    public int HeavysideActivation(double value) => value < 0 ? 0 : 1;

    public void ResetWeights()
    {
        for (int i = 0; i < this.Weights.Length; i++)
        {
            this.Weights[i] = this.Seed.NextDouble();
        }
    }

    public Random Seed { get; set; } = new Random();
    public double[] Weights { get; set; }
    public double LearningRate { get; set; }

    public PerceptronClassifer(double learningRate = 0.125)
    {
        this.LearningRate = learningRate;
    }

    public virtual List<int> Predict(List<ClassificationExample> examples)
    {
        var predictions = new List<int>();
        foreach (ClassificationExample example in examples)
        {
            var weightedFeatures = example
                .FeaturesWithBias()
                .Select((feature, i) => feature * this.Weights[i])
                .Sum();
            var prediction = this.HeavysideActivation(weightedFeatures);
            predictions.Add(prediction);
        }
        return predictions;
    }

    // we do async training, i.e. update weights on every example (aka min
    // batch)
    virtual public void Fit(List<ClassificationExample> examples)
    {
        this.Weights = new double[examples.First().FeaturesWithBias().Count()];
        this.ResetWeights();

        for (int i = 0; i < examples.Count; i++)
        {
            var example = examples[i];
            var predictableExample = Enumerable.Repeat(examples[i], 1).ToList();
            var predictedLabel = this.Predict(predictableExample).First();
            var error = example.Label - predictedLabel;

            // update bias and weights
            this.Weights[0] = Weights[0] + LearningRate * error;
            for (int j = 1; j < this.Weights.Length; j++)
            {
                Weights[j] = Weights[j] + LearningRate * error * example.Features[j - 1];
            }
        }
    }

    public override List<ClassificationExample> Transform(List<ClassificationExample> examples)
    {
        return Predict(examples)
            .Zip(
                examples,
                (predictedLabel, example) =>
                    new ClassificationExample(predictedLabel, example.Features)
            )
            .ToList();
    }

    public List<ClassificationExample> FitTransform(List<ClassificationExample> examples)
    {
        Fit(examples);
        return Transform(examples);
    }
}
