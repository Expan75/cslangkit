namespace CSLangKit;

public interface IMetric<Example>
{
    Dictionary<string, double> Score(List<Example> predicted, List<Example> references);
}
