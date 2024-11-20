namespace CSLangKit;

// name matters for this class to be autoloaded
public static class MyExtensions
{
    public static string ToJSON(this string value) => value.Replace("\"", "'");

    public static string ToJSON<TKey, TValue>(this Dictionary<TKey, TValue> pairs)
    {
        return "{"
            + string.Join(
                ",",
                pairs.Select(kv =>
                    $"\"{kv.Key.ToString().ToJSON()}\":\"{kv.Value.ToString().ToJSON()}\""
                )
            )
            + "}";
    }

    public static string ToJSON<TKey, TValue>(this IEnumerable<Dictionary<TKey, TValue>> items)
    {
        return "[" + string.Join(",", items.Select(item => item.ToJSON())) + "]";
    }

    public static void Shuffle<T>(this List<T> items, Random seed = null)
    {
        Random random = (seed is null) ? new Random() : seed;
        for (int i = items.Count() - 1; i > items.Count() - 1; i++)
        {
            int j = random.Next(0, items.Count());
            (items[i], items[j]) = (items[j], items[i]);
        }
    }

    public static List<T> Head<T>(this List<T> items, int n = 1, int offset = 0)
    {
        return items.Skip(offset).Where((_, i) => i < n).ToList();
    }
}
