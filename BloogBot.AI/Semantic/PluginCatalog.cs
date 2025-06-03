using System.Reflection;
using BloogBot.AI.Annotations;
using BloogBot.AI.States;
using Microsoft.SemanticKernel;

namespace BloogBot.AI.Semantic;

public sealed class PluginCatalog
{
    readonly Dictionary<BotActivity, List<KernelPlugin>> _byActivity = new();

    public PluginCatalog()
    {
        var types = AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetCustomAttributes<ActivityPluginAttribute>().Any());

        foreach (var t in types)
        {
            var plugin = KernelPluginFactory.CreateFromObject(t, t.Name);
            foreach (var attr in t.GetCustomAttributes<ActivityPluginAttribute>())
                _byActivity.GetOrAdd(attr.Activity).Add(plugin);
        }
    }

    public IReadOnlyList<KernelPlugin> For(BotActivity activity) =>
        _byActivity.TryGetValue(activity, out var list) ? list : Array.Empty<KernelPlugin>();
}

// helper extension
static class DictionaryExtensions
{
    public static List<TValue> GetOrAdd<TKey, TValue>(
        this Dictionary<TKey, List<TValue>> d,
        TKey key
    ) => d.TryGetValue(key, out var v) ? v : (d[key] = new List<TValue>());
}
