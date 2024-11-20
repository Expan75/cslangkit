namespace CSLangKit;

using System.Text.RegularExpressions;

public class RegexTokenizer : ITokenizer
{
    private static HashSet<char> ReservedDelimiters = new HashSet<char>() { '?', '!', ',', '.' };

    private const string VOCAB_ADDABLE_TOKEN = @"[^\s]+";
    private const string UNKNOWN_FLAG = "idk";
    private const RegexOptions FILTER_OPTIONS = RegexOptions.Compiled | RegexOptions.Multiline;
    private static Regex BaseFilter { get; set; } =
        new Regex(VOCAB_ADDABLE_TOKEN, FILTER_OPTIONS, TimeSpan.FromMilliseconds(10));

    // helper for breaking up <word><delimiter> pairs like "Sure, idk... go
    // ahead and try
    private bool RequiresDelimiterExpansion(string raw)
    {
        var delimiterSuffix = raw[raw.Length - 1];
        bool canRemoveDelimiterSuffix =
            ReservedDelimiters.Contains(delimiterSuffix)
            && (raw.Length > 2)
            && (!ReservedDelimiters.Contains(raw[raw.Length - 2]));

        return canRemoveDelimiterSuffix;
    }

    private (string, string) Normalize(string raw)
    {
        if (this.RequiresDelimiterExpansion(raw))
        {
            return (raw.Substring(0, raw.Length - 1), raw[raw.Length - 1].ToString());
        }
        return (raw, "");
    }

    public List<string> ReversedVocabulary { get; set; } = new List<string>();
    public Dictionary<string, int> Vocabulary { get; set; } = new Dictionary<string, int>();

    public RegexTokenizer() { }

    public string Decode(List<int> tokens) =>
        tokens.Select(i => this.ReversedVocabulary[i]).ToString();

    public List<int> Encode(string document)
    {
        // while regex works well, it does aqeduately seperate end or
        // intra-sentence delimiters like '?', '!', '.'. We adjust our token
        // matching to account for this via simple heuristic.
        var tokens = new List<int>();
        Dictionary<int, int> offsets = new Dictionary<int, int>();
        foreach (Match match in BaseFilter.Matches(document))
        {
            var (prefix, suffix) = this.Normalize(match.Value);
            if (this.Vocabulary.ContainsKey(prefix))
            {
                offsets[match.Index] = prefix.Length;
            }

            if (this.Vocabulary.ContainsKey(suffix))
            {
                offsets[match.Index + prefix.Length] = 1;
            }
        }

        // go character by character assigning unknown OR encode and skip ahead on
        // recognised tokens
        for (int i = 0; i < document.Length; i++)
        {
            bool atRecognisedToken = offsets.ContainsKey(i);
            if (atRecognisedToken)
            {
                string tokenLiteral = document.Substring(i, offsets[i]);
                var (normalisedLiteral, _) = Normalize(tokenLiteral);
                int token = this.Vocabulary[normalisedLiteral];
                i += offsets[i];
                tokens.Add(token);
            }
            else
            {
                tokens.Add(this.Vocabulary[UNKNOWN_FLAG]);
            }
        }
        return tokens;
    }

    public void Fit(List<string> corpus)
    {
        HashSet<string> tokens = new HashSet<string>();
        foreach (string document in corpus)
        {
            foreach (Match token in BaseFilter.Matches(document))
            {
                var (prefix, suffix) = this.Normalize(token.Value);
                tokens.Add(prefix);
                if (suffix.Length == 1)
                {
                    tokens.Add(suffix);
                }
            }
        }

        // we also add an "unknown" token to signal things not covered.
        tokens.Add(UNKNOWN_FLAG);
        this.ReversedVocabulary = tokens.OrderByDescending(t => t.Length).ToList();
        for (int i = 0; i < tokens.Count(); i++)
        {
            string tokenLiteral = this.ReversedVocabulary[i];
            this.Vocabulary[tokenLiteral] = i;
        }
    }

    public List<List<int>> Transform(List<string> documents) =>
        documents.Select(d => Encode(d)).ToList();

    public List<List<int>> FitTransform(List<string> corpus)
    {
        this.Fit(corpus);
        return this.Transform(corpus);
    }
}
