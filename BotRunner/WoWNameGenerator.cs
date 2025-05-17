using GameData.Core.Enums;
using System;
using System.Collections.Generic;

namespace BotRunner
{
    public static class WoWNameGenerator
    {
        private static readonly Random rng = new();

        public static string GenerateName(Race race, Gender gender)
        {
            var (prefixes, middles, suffixes) = GetSyllables(race, gender);

            string prefix = prefixes[rng.Next(prefixes.Count)];
            string middle = middles[rng.Next(middles.Count)];
            string suffix = suffixes[rng.Next(suffixes.Count)];
            string constructedName = prefix + middle + suffix;

            return constructedName;
        }

        public static Race ParseRaceCode(string code)
        {
            foreach (var kvp in RaceCodeMap)
            {
                if (kvp.Value.Equals(code, StringComparison.OrdinalIgnoreCase))
                    return kvp.Key;
            }
            throw new ArgumentException($"Unknown race code: {code}");
        }

        public static Class ParseClassCode(string code)
        {
            foreach (var kvp in ClassCodeMap)
            {
                if (kvp.Value.Equals(code, StringComparison.OrdinalIgnoreCase))
                    return kvp.Key;
            }
            throw new ArgumentException($"Unknown class code: {code}");
        }

        public static Gender DetermineGender(Class @class)
        {
            return @class switch
            {
                Class.Mage or Class.Warlock or Class.Priest or Class.Druid or Class.Shaman => Gender.Male,
                _ => Gender.Female
            };
        }

        private static (List<string>, List<string>, List<string>) GetSyllables(Race race, Gender gender)
        {
            string key = race.ToString() + gender.ToString();
            return SyllableData.All[key];
        }

        private static readonly Dictionary<Race, string> RaceCodeMap = new()
        {
            { Race.Human, "HU" },
            { Race.Dwarf, "DW" },
            { Race.NightElf, "NE" },
            { Race.Gnome, "GN" },
            { Race.Orc, "OR" },
            { Race.Undead, "UD" },
            { Race.Tauren, "TA" },
            { Race.Troll, "TR" }
        };

        private static readonly Dictionary<Class, string> ClassCodeMap = new()
        {
            { Class.Warrior, "WR" },
            { Class.Paladin, "PA" },
            { Class.Rogue, "RO" },
            { Class.Hunter, "HU" },
            { Class.Mage, "MA" },
            { Class.Warlock, "WL" },
            { Class.Priest, "PR" },
            { Class.Druid, "DR" },
            { Class.Shaman, "SH" }
        };

        public static class SyllableData
        {
            public static readonly Dictionary<string, (List<string>, List<string>, List<string>)> All = new()
            {
                ["HumanMale"] = (["Al", "Ced", "God", "Tar", "Alb", "Rol", "Val", "Max", "Con", "Bla"], ["ri", "e", "ow", "ar", "br", "an", "el", "or", "in", "ed"], ["ic", "ric", "win", "ren", "cht", "ard", "us", "ton", "den", "mar"]),
                ["HumanFemale"] = (["El", "Mae", "Row", "Iso", "Brie", "Lea", "Aur", "Is", "Mir", "Sar"], ["ly", "er", "we", "so", "en", "an", "el", "ar", "in", "ia"], ["nna", "ra", "na", "lde", "ia", "elle", "ine", "ora", "ita", "esa"]),
                ["DwarfMale"] = (["Bro", "Thr", "Mag", "Dor", "Khar", "Kaz", "Thro", "Gar", "Bal", "Dur"], ["ga", "ai", "ni", "or", "ve", "in", "ar", "um", "ek", "ol"], ["r", "in", "ni", "n", "ek", "grim", "son", "dor", "bek", "thal"]),
                ["DwarfFemale"] = (["Bru", "Hel", "Kat", "Fre", "An", "Bry", "Mor", "Dis", "Tor", "Hil"], ["ni", "ga", "ra", "ya", "ja", "el", "an", "or", "is", "ul"], ["a", "la", "ra", "na", "ja", "wyn", "ith", "da", "lin", "mira"]),
                ["NightElfMale"] = (["Thal", "Kel", "Elu", "Fae", "Than", "Elen", "Lor", "Mal", "Syl", "Val"], ["da", "va", "lu", "ae", "do", "ri", "an", "el", "il", "or"], ["nil", "ras", "ien", "lar", "ril", "dor", "thas", "nor", "sil", "ven"]),
                ["NightElfFemale"] = (["Mai", "Sir", "Aly", "Lor", "Sha", "Tyr", "Sel", "Alu", "Myr", "Nym"], ["ev", "ae", "nd", "ra", "lu", "el", "an", "il", "or", "ia"], ["ra", "el", "dra", "thal", "nae", "ria", "lia", "wyn", "thea", "mira"]),
                ["GnomeMale"] = (["Fiz", "Gear", "Quib", "Sniz", "Cog", "Blim", "Tink", "Zep", "Glim", "Nib"], ["wiz", "bin", "ble", "gur", "gle", "ton", "zap", "nix", "cog", "spro"], ["zle", "in", "ton", "t", "pop", "wick", "bert", "dink", "fizz", "bolt"]),
                ["GnomeFemale"] = (["Tin", "Bub", "Giz", "Sni", "Frin", "Pip", "Trix", "Zani", "Lili", "Nani"], ["kwi", "bli", "zet", "ppi", "zel", "ette", "ina", "ola", "eli", "umi"], ["na", "etta", "te", "pa", "el", "belle", "mimi", "lynn", "dora", "nique"]),
                ["OrcMale"] = (["Grom", "Dro", "Mur", "Thro", "Lok", "Gar", "Karg", "Naz", "Thok", "Zul"], ["gas", "ro", "gan", "gar", "thar", "mok", "zug", "dak", "rok", "urg"], ["sh", "g", "n", "ar", "ok", "gor", "thak", "zug", "mok", "dur"]),
                ["OrcFemale"] = (["Gor", "Thru", "Dral", "Mog", "Rag", "Gra", "Urg", "Sha", "Lok", "Zog"], ["ga", "ka", "ga", "rah", "na", "dra", "mog", "zug", "kar", "sha"], ["ra", "ka", "ga", "rah", "a", "gra", "thra", "mok", "dur", "sha"]),
                ["UndeadMale"] = (["Var", "Mal", "Sep", "Thor", "Car", "Nec", "Mor", "Dra", "Luc", "Zom"], ["n", "tr", "hek", "ne", "an", "oth", "il", "ar", "um", "ex"], ["x", "lus", "ek", "ne", "ox", "th", "us", "or", "is", "en"]),
                ["UndeadFemale"] = (["Sy", "Mor", "Ela", "Dre", "Van", "Lil", "Nec", "Isa", "Mar", "Bel"], ["bil", "va", "ne", "lia", "is", "eth", "ora", "ina", "ula", "ela"], ["le", "a", "th", "ia", "sa", "ine", "ora", "esa", "ula", "elle"]),
                ["TaurenMale"] = (["Ho", "Mak", "Rune", "Ta", "Kor", "Bai", "Gra", "Hul", "Mor", "Tor"], ["kan", "wa", "tot", "hu", "ga", "run", "tho", "mar", "lak", "dor"], ["n", "a", "em", "u", "ga", "horn", "hoof", "tusk", "mane", "tail"]),
                ["TaurenFemale"] = (["Sha", "Ahw", "Tey", "Nash", "Elu", "Mai", "Ana", "Ora", "Luma", "Sira"], ["noa", "ana", "la", "hi", "lu", "sha", "lia", "mira", "nora", "thea"], ["a", "na", "ra", "sha", "a", "wyn", "mira", "lia", "thea", "nara"]),
                ["TrollMale"] = (["Zal", "Rok", "Vol", "Daz", "Mak", "Zul", "Jin", "Gor", "Tal", "Nak"], ["jin", "han", "kur", "tal", "jam", "rak", "zul", "mar", "dak", "lor"], ["bo", "ok", "o", "ul", "ur", "jin", "rok", "thul", "zan", "mok"]),
                ["TrollFemale"] = (["Zen", "Rok", "Ta", "Zu", "Kal", "Sha", "Vol", "Nal", "Zil", "Ora"], ["za", "ka", "ji", "la", "ji", "sha", "mira", "nora", "thea", "lia"], ["li", "ra", "i", "ya", "ra", "wyn", "mira", "lia", "thea", "nara"])
            };
        }
    }
}
