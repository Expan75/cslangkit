namespace CSLangKit;

using System.Collections.Generic;

public class ExtendedBPETokenizer : ITokenizer
{
    private int TargetVocabularySize { get; set; }
    public List<string> ReversedVocabulary { get; set; } = new List<string>();
    public Dictionary<string, int> Vocabulary { get; set; } = new Dictionary<string, int>();
    public Dictionary<(int, int), int> Merges { get; set; } = new Dictionary<(int, int), int>();

    public ExtendedBPETokenizer(int targetVocabularySize)
    {
        this.TargetVocabularySize = targetVocabularySize;
    }

    private List<int> Merge(IEnumerable<int> tokens)
    {
        var mergedTokens = new List<int>();
        var candidateTokens = new LinkedList<int>(tokens);

        while (candidateTokens.Count() > 0)
        {
            if (candidateTokens.Count() >= 2) // can only merge pairs
            {
                var head = (candidateTokens.First(), candidateTokens.Skip(1).First());
                if (this.Merges.ContainsKey(head))
                {
                    int mergedToken = this.Merges[head];
                    candidateTokens.RemoveFirst();
                    candidateTokens.RemoveFirst();
                    candidateTokens.AddFirst(mergedToken); // recurse upon new start!
                }
            }
            else
            {
                var token = candidateTokens.Last();
                candidateTokens.Clear();
                mergedTokens.Add(token);
            }
        }
        return mergedTokens.ToList();
    }

    private Dictionary<(int, int), int> CountIndexPairs(IEnumerable<int> tokens)
    {
        var counts = new Dictionary<(int, int), int>();
        foreach (var tokenPair in tokens.Zip(tokens.Skip(1), (l, r) => (l, r)))
        {
            counts[tokenPair] = counts.ContainsKey(tokenPair) ? counts[tokenPair] + 1 : 1;
        }
        return counts;
    }

    private (int, int) GetNextMergeRule(IEnumerable<int> corpus)
    {
        var maxCount = 0;
        var nextMergeRule = (-1, -1);

        foreach (var pairCount in this.CountIndexPairs(corpus))
        {
            if (pairCount.Value > maxCount)
            {
                maxCount = pairCount.Value;
                nextMergeRule = pairCount.Key;
            }
        }
        return nextMergeRule;
    }

    public List<int> Encode(string document)
    {
        List<int> tokens = document.Select(c => this.Vocabulary[c.ToString()]).ToList();
        return this.Merge(tokens);
    }

    public string Decode(List<int> tokens) =>
        string.Join(" ", tokens.Select(i => this.ReversedVocabulary[i]));

    public void Fit(List<string> corpus)
    {
        // first pass is always adding all unique characters
        foreach (char token in corpus.SelectMany(c => c).Distinct())
        {
            this.Vocabulary[token.ToString()] = this.ReversedVocabulary.Count;
            this.ReversedVocabulary.Add(token.ToString());
        }

        while (this.Vocabulary.Count < this.TargetVocabularySize)
        {
            var corpusTokens = corpus.Select(d => this.Encode(d)).SelectMany(tokens => tokens);
            var (former, latter) = this.GetNextMergeRule(corpusTokens);
            string mergedTokenLiteral =
                this.ReversedVocabulary[former] + this.ReversedVocabulary[latter];

            // each "merge" rule is a new token
            this.Vocabulary[mergedTokenLiteral] = this.ReversedVocabulary.Count;
            this.ReversedVocabulary.Add(mergedTokenLiteral);
        }
    }

    public List<List<int>> Transform(List<string> documents) =>
        documents.Select(d => this.Encode(d)).ToList();

    public List<List<int>> FitTransform(List<string> documents)
    {
        this.Fit(documents);
        return this.Transform(documents);
    }
}
