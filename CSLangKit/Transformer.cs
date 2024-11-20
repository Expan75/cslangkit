namespace CSLangKit;

public interface ITransformer<In, Out>
{
    void Fit(List<In> records);
    List<Out> Transform(List<In> records);
    List<Out> FitTransform(List<In> records);
}
