namespace CSLangKit;

public interface ITokenizer : ITransformer<string, List<int>>
{
    Dictionary<string, int> Vocabulary { get; set; }
    List<int> Encode(string document);
    string Decode(List<int> encodedDocument);
}
