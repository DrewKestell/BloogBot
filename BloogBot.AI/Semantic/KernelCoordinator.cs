using BloogBot.AI.Semantic;
using BloogBot.AI.States;
using Microsoft.SemanticKernel;

public sealed class KernelCoordinator
{
    private readonly Kernel _kernel;
    private readonly PluginCatalog _catalog;

    public KernelCoordinator(Kernel kernel, PluginCatalog catalog)
    {
        _kernel = kernel;
        _catalog = catalog;
    }

    public void OnActivityChanged(BotActivity newActivity)
    {
        _kernel.Plugins.Clear();
        foreach (var p in _catalog.For(newActivity))
            _kernel.Plugins.Add(p);
    }
}
