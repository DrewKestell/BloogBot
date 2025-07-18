namespace BloogBot.AI.Semantic;

static class DictionaryExtensions
{
    public static List<TValue> GetOrAdd<TKey, TValue>(
        this Dictionary<TKey, List<TValue>> d,
        TKey key
    ) => d.TryGetValue(key, out var v) ? v : (d[key] = new List<TValue>());
}
