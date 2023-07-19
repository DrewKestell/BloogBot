using BloogBot.AI;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BloogBot
{
    class BotLoader
    {
        readonly IDictionary<string, string> assemblies = new Dictionary<string, string>();

#pragma warning disable 0649
        [ImportMany(typeof(IBot), AllowRecomposition = true)]
        List<IBot> bots;
#pragma warning restore 0649

        AggregateCatalog catalog = new AggregateCatalog();
        CompositionContainer container;

        public BotLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => { 
                
                var currentAssembly = Assembly.GetExecutingAssembly();
                var name = args.Name.Split(',')[0];
                var assembly = Assembly.Load(name) ?? currentAssembly;
                return assembly;
            };
        }

        internal List<IBot> ReloadBots()
        {
            bots?.Clear();
            container?.Dispose();

            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var botPaths = new[] { "AfflictionWarlockBot.dll", "ArcaneMageBot.dll", "ArmsWarriorBot.dll", "BackstabRogueBot.dll", "BalanceDruidBot.dll", "BeastMasterHunterBot.dll", "CombatRogueBot.dll", "EnhancementShamanBot.dll", "ElementalShamanBot.dll", "FeralDruidBot.dll", "FrostMageBot.dll", "FuryWarriorBot.dll", "ProtectionPaladinBot.dll", "ProtectionWarriorBot.dll", "RetributionPaladinBot.dll", "ShadowPriestBot.dll", "TestBot.dll" };

            foreach (var botPath in botPaths)
            {
                var path = Path.Combine(currentFolder, botPath);
                var assembly = Assembly.Load(File.ReadAllBytes(path));
                var assemblyName = assembly.FullName.Split(',')[0];
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
            }
            container = new CompositionContainer(catalog);
            container.ComposeParts(this);

            return bots
                .GroupBy(b => b.Name)
                .Select(b => b.Last())
                .ToList();
        }
    }
}
