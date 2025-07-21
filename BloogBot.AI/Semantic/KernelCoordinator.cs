using BloogBot.AI.Semantic;
using BloogBot.AI.States;
using Microsoft.SemanticKernel;

public sealed class KernelCoordinator(Kernel kernel, PluginCatalog catalog)
{
    private readonly Kernel _kernel = kernel;
    private readonly PluginCatalog _catalog = catalog;

    public void OnActivityChanged(BotActivity newActivity)
    {
        _kernel.Plugins.Clear();
        foreach (var p in _catalog.For(newActivity))
            _kernel.Plugins.Add(p);
    }
}
