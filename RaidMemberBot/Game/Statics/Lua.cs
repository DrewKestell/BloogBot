using RaidMemberBot.Mem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.Game.Statics
{
    /// <summary>
    ///     Class regarding Lua stuff
    /// </summary>
    public class Lua
    {
        private static readonly Lazy<Lua> _instance = new Lazy<Lua>(() => new Lua());

        private static readonly Random Random = new Random();

        private Lua()
        {
        }

        /// <summary>
        ///     Access to the current instance
        /// </summary>
        /// <value>
        ///     The instance.
        /// </value>
        public static Lua Instance => _instance.Value;

        /// <summary>
        ///     Executes Lua code
        /// </summary>
        /// <example>
        ///     <code>Execute("DoEmote('dance')");</code>
        /// </example>
        /// <param name="parScript">The code</param>
        public void Execute(string parScript)
        {
            Functions.DoString(parScript);
        }

        /// <summary>
        ///     Will execute a Lua script and return values assigned to all variables created by placeholders:
        ///     {0} = 'hello', {1} = 'world' => Result will be { "hello", "world" }
        /// </summary>
        /// <param name="parScript">The Lua script</param>
        /// <returns>The string return values</returns>
        public string[] ExecuteWithResult(string parScript)
        {
            List<string> luaVarNames = new List<string>();
            for (int i = 0; i < 11; i++)
            {
                string currentPlaceHolder = "{" + i + "}";
                if (!parScript.Contains(currentPlaceHolder)) break;
                string randomName = GetRandomLuaVarName();
                parScript = parScript.Replace(currentPlaceHolder, randomName);
                luaVarNames.Add(randomName);
            }
            return ThreadSynchronizer.Instance.Invoke(() =>
            {
                Functions.DoString(parScript);
                return Functions.GetText(luaVarNames.ToArray());
            });
        }

        private static string GetRandomLuaVarName()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(chars.Select(c => chars[Random.Next(chars.Length)]).Take(8).ToArray());
        }
    }
}
