using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Runtime.Loader;
using BotRunner.Interfaces;

namespace BotRunner
{
    internal class BotLoader
    {
        private readonly IDictionary<string, string> assemblies = new Dictionary<string, string>();

#pragma warning disable 0649
        [ImportMany(typeof(IBot), AllowRecomposition = true)]
        private readonly List<IBot> bots;
#pragma warning restore 0649

        private readonly AggregateCatalog catalog = new();
        private CompositionContainer container;

        public BotLoader()
        {
        }

        internal List<IBot> ReloadBots()
        {
            bots?.Clear();
            container?.Dispose();

            string currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string[] botPaths = [
                "AfflictionWarlockBot.dll",
                "ArcaneMageBot.dll",
                "ArmsWarriorBot.dll",
                "BackstabRogueBot.dll",
                "DruidBalance.dll",
                "BeastMasterHunterBot.dll",
                "CombatRogueBot.dll",
                "ElementalShamanBot.dll",
                "EnhancementShamanBot.dll",
                "DruidFeral.Tasks.dll",
                "FrostMageBot.dll",
                "FuryWarriorBot.dll",
                "ProtectionPaladinBot.dll",
                "ProtectionWarriorBot.dll",
                "RetributionPaladinBot.dll",
                "ShadowPriestBot.dll"
            ];

            foreach (string botPath in botPaths)
            {
                try
                {
                    string path = GetAssemblyPath(currentFolder, botPath);

                    if (path == null)
                    {
                        throw new FileNotFoundException($"BOT LOADER: {botPath} not found.");
                    }

                    var assembly = LoadAssembly(path);
                    string assemblyName = assembly.FullName.Split(',')[0];
                    if (assemblies.TryGetValue(assemblyName, out string? value))
                    {
                        if (value != assembly.FullName)
                        {
                            catalog.Catalogs.Add(new AssemblyCatalog(assembly));
                            assemblies[assemblyName] = assembly.FullName;
                        }
                    }
                    else
                    {
                        catalog.Catalogs.Add(new AssemblyCatalog(assembly));
                        assemblies.Add(assemblyName, assembly.FullName);
                    }
                    container = new CompositionContainer(catalog);
                    container.ComposeParts(this);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Console.WriteLine($"BOT LOADER: {ex.StackTrace}");
                    foreach (Exception inner in ex.LoaderExceptions)
                    {
                        Console.WriteLine($"BOT LOADER: {inner.Message}");
                    }
                }
            }

            return bots
                .GroupBy(b => b.Name)
                .Select(b => b.Last())
                .ToList();
        }

        private static string GetAssemblyPath(string currentFolder, string botPath)
        {
            string[] searchPaths = [
                currentFolder,
                Path.Combine(Directory.GetParent(currentFolder).FullName),
                Path.Combine(currentFolder, "Debug"),
                Path.Combine(currentFolder, "Release")
            ];

            foreach (var searchPath in searchPaths)
            {
                var path = Path.Combine(searchPath, botPath);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private static Assembly LoadAssembly(string path)
        {
            var alc = new AssemblyLoadContext(path, true);
            return alc.LoadFromAssemblyPath(path);
        }
    }
}
