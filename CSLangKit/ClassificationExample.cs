namespace CSLangKit;

public class ClassificationExample
{
    public int Label { private set; get; }
    public double[] Features { private set; get; }

    public ClassificationExample(int label, double[] features)
    {
        this.Label = label;
        this.Features = features;
    }

    public IEnumerable<double> FeaturesWithBias()
    {
        for (int i = -1; i < this.Features.Length; i++)
        {
            yield return (i == -1) ? 1 : this.Features[i];
        }
    }
}
