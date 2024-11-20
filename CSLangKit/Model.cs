namespace CSLangKit;

public interface IModel<Example> : ITransformer<Example, Example>
{
    double[] Weights { get; set; }
    List<int> Predict(List<Example> examples);
    Dictionary<string, double> Score(List<Example> examples);
}
