using WoWActivityMember.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace WoWActivityMember
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
                "ShadowPriestBot.dll"];

            foreach (string botPath in botPaths)
            {
                try
                {
                    string path = Path.Combine(currentFolder, botPath);

                    if (!File.Exists(path))
                    {
                        var parent = Directory.GetParent(currentFolder);
                        path = Path.Combine(parent.FullName, botPath);
                    }

                    if (!File.Exists(path))
                    {
                        path = Path.Combine(currentFolder, "Debug", botPath);
                    }

                    if (!File.Exists(path))
                    {
                        path = Path.Combine(currentFolder, "Release", botPath);
                    }

                    if (!File.Exists(path))
                    {
                        throw new FileNotFoundException($"BOT LOADER: {botPath} not found.");
                    }

                    Assembly assembly = Assembly.Load(File.ReadAllBytes(path));
                    string assemblyName = assembly.FullName.Split(',')[0];
                    if (assemblies.ContainsKey(assemblyName))
                    {
                        if (assemblies[assemblyName] != assembly.FullName)
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
                    // now look at ex.LoaderExceptions - this is an Exception[], so:
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
    }
}
