using System;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace RaidLeaderBot
{
    public class AccountUsageRegistry
    {
        private static readonly Lazy<AccountUsageRegistry> _instance =
            new Lazy<AccountUsageRegistry>(() => new AccountUsageRegistry());
        public static AccountUsageRegistry Instance => _instance.Value;
        private AccountUsageRegistry() { }
        public string CheckoutNextAvaiableAccountName(Race race, Class classId)
        {
            foreach (var keyValuePair in _accountUsageDictionary)
            {
                if (keyValuePair.Key.Item1 == race && keyValuePair.Key.Item2 == classId)
                {
                    foreach (var accountList in keyValuePair.Value)
                    {
                        if (!accountList.Value)
                        {
                            keyValuePair.Value[accountList.Key] = true;

                            return accountList.Key;
                        }
                    }
                }
            }
            return "OrWr1";
        }
        public void CheckoutNextAvaiableAccountName(Race race, Class classId, string accountName)
        {
            foreach (var keyValuePair in _accountUsageDictionary)
            {
                if (keyValuePair.Key.Item1 == race && keyValuePair.Key.Item2 == classId)
                {
                    keyValuePair.Value[accountName] = false;
                    break;
                }
            }
        }
        private Dictionary<Tuple<Race, Class>, Dictionary<string, bool>> _accountUsageDictionary = new Dictionary<Tuple<Race, Class>, Dictionary<string, bool>>() {
            {
                Tuple.Create(Race.Human, Class.Warrior), new Dictionary<string, bool>()
                {
                    {"HuWr1", false},
                    {"HuWr2", false},
                    {"HuWr3", false},
                    {"HuWr4", false},
                    {"HuWr5", false},
                    {"HuWr6", false},
                    {"HuWr7", false},
                    {"HuWr8", false},
                    {"HuWr9", false},
                    {"HuWr10", false}
                }
            },
            { Tuple.Create(Race.Human, Class.Paladin), new Dictionary<string, bool>()
                {
                    {"HuPa1", false},
                    {"HuPa2", false},
                    {"HuPa3", false},
                    {"HuPa4", false},
                    {"HuPa5", false},
                    {"HuPa6", false},
                    {"HuPa7", false},
                    {"HuPa8", false},
                    {"HuPa9", false},
                    {"HuPa10", false}
                }
            },
            { Tuple.Create(Race.Human, Class.Rogue), new Dictionary<string, bool>()
                {
                    {"HuRo1", false},
                    {"HuRo2", false},
                    {"HuRo3", false},
                    {"HuRo4", false},
                    {"HuRo5", false},
                    {"HuRo6", false},
                    {"HuRo7", false},
                    {"HuRo8", false},
                    {"HuRo9", false},
                    {"HuRo10", false}
                }
            },
            { Tuple.Create(Race.Human, Class.Priest), new Dictionary<string, bool>()
                {
                    {"HuPr1", false},
                    {"HuPr2", false},
                    {"HuPr3", false},
                    {"HuPr4", false},
                    {"HuPr5", false},
                    {"HuPr6", false},
                    {"HuPr7", false},
                    {"HuPr8", false},
                    {"HuPr9", false},
                    {"HuPr10", false}
                }
            },
            { Tuple.Create(Race.Human, Class.Mage), new Dictionary<string, bool>()
                {
                    {"HuMa1", false},
                    {"HuMa2", false},
                    {"HuMa3", false},
                    {"HuMa4", false},
                    {"HuMa5", false},
                    {"HuMa6", false},
                    {"HuMa7", false},
                    {"HuMa8", false},
                    {"HuMa9", false},
                    {"HuMa10", false}
                }
            },
            { Tuple.Create(Race.Human, Class.Warlock), new Dictionary<string, bool>()
                {
                    {"HuWl1", false},
                    {"HuWl2", false},
                    {"HuWl3", false},
                    {"HuWl4", false},
                    {"HuWl5", false},
                    {"HuWl6", false},
                    {"HuWl7", false},
                    {"HuWl8", false},
                    {"HuWl9", false},
                    {"HuWl10", false}
                }
            },

            { Tuple.Create(Race.Dwarf, Class.Warrior), new Dictionary<string, bool>()
                {
                    {"DwWr1", false},
                    {"DwWr2", false},
                    {"DwWr3", false},
                    {"DwWr4", false},
                    {"DwWr5", false},
                    {"DwWr6", false},
                    {"DwWr7", false},
                    {"DwWr8", false},
                    {"DwWr9", false},
                    {"DwWr10", false}
                }
            },
            { Tuple.Create(Race.Dwarf, Class.Paladin), new Dictionary<string, bool>()
                {
                    {"DwPa1", false},
                    {"DwPa2", false},
                    {"DwPa3", false},
                    {"DwPa4", false},
                    {"DwPa5", false},
                    {"DwPa6", false},
                    {"DwPa7", false},
                    {"DwPa8", false},
                    {"DwPa9", false},
                    {"DwPa10", false}
                }
            },
            { Tuple.Create(Race.Dwarf, Class.Hunter), new Dictionary<string, bool>()
                {
                    {"DwHu1", false},
                    {"DwHu2", false},
                    {"DwHu3", false},
                    {"DwHu4", false},
                    {"DwHu5", false},
                    {"DwHu6", false},
                    {"DwHu7", false},
                    {"DwHu8", false},
                    {"DwHu9", false},
                    {"DwHu10", false}
                }
            },
            { Tuple.Create(Race.Dwarf, Class.Rogue), new Dictionary<string, bool>()
                {
                    {"DwRo1", false},
                    {"DwRo2", false},
                    {"DwRo3", false},
                    {"DwRo4", false},
                    {"DwRo5", false},
                    {"DwRo6", false},
                    {"DwRo7", false},
                    {"DwRo8", false},
                    {"DwRo9", false},
                    {"DwRo10", false}
                }
            },
            { Tuple.Create(Race.Dwarf, Class.Priest), new Dictionary<string, bool>()
                {
                    {"DwPr1", false},
                    {"DwPr2", false},
                    {"DwPr3", false},
                    {"DwPr4", false},
                    {"DwPr5", false},
                    {"DwPr6", false},
                    {"DwPr7", false},
                    {"DwPr8", false},
                    {"DwPr9", false},
                    {"DwPr10", false}
                }
            },

            { Tuple.Create(Race.NightElf, Class.Warrior), new Dictionary<string, bool>()
                {
                    {"NiWr1", false},
                    {"NiWr2", false},
                    {"NiWr3", false},
                    {"NiWr4", false},
                    {"NiWr5", false},
                    {"NiWr6", false},
                    {"NiWr7", false},
                    {"NiWr8", false},
                    {"NiWr9", false},
                    {"NiWr10", false}
                }
            },
            { Tuple.Create(Race.NightElf, Class.Hunter), new Dictionary<string, bool>()
                {
                    {"NiHu1", false},
                    {"NiHu2", false},
                    {"NiHu3", false},
                    {"NiHu4", false},
                    {"NiHu5", false},
                    {"NiHu6", false},
                    {"NiHu7", false},
                    {"NiHu8", false},
                    {"NiHu9", false},
                    {"NiHu10", false}
                }
            },
            { Tuple.Create(Race.NightElf, Class.Rogue), new Dictionary<string, bool>()
                {
                    {"NiRo1", false},
                    {"NiRo2", false},
                    {"NiRo3", false},
                    {"NiRo4", false},
                    {"NiRo5", false},
                    {"NiRo6", false},
                    {"NiRo7", false},
                    {"NiRo8", false},
                    {"NiRo9", false},
                    {"NiRo10", false}
                }
            },
            { Tuple.Create(Race.NightElf, Class.Priest), new Dictionary<string, bool>()
                {
                    {"NiPr1", false},
                    {"NiPr2", false},
                    {"NiPr3", false},
                    {"NiPr4", false},
                    {"NiPr5", false},
                    {"NiPr6", false},
                    {"NiPr7", false},
                    {"NiPr8", false},
                    {"NiPr9", false},
                    {"NiPr10", false}
                }
            },
            { Tuple.Create(Race.NightElf, Class.Druid), new Dictionary<string, bool>()
                {
                    {"NiDr1", false},
                    {"NiDr2", false},
                    {"NiDr3", false},
                    {"NiDr4", false},
                    {"NiDr5", false},
                    {"NiDr6", false},
                    {"NiDr7", false},
                    {"NiDr8", false},
                    {"NiDr9", false},
                    {"NiDr10", false}
                }
            },

            { Tuple.Create(Race.Gnome, Class.Warrior), new Dictionary<string, bool>()
                {
                    {"GnWr1", false},
                    {"GnWr2", false},
                    {"GnWr3", false},
                    {"GnWr4", false},
                    {"GnWr5", false},
                    {"GnWr6", false},
                    {"GnWr7", false},
                    {"GnWr8", false},
                    {"GnWr9", false},
                    {"GnWr10", false}
                }
            },
            { Tuple.Create(Race.Gnome, Class.Mage), new Dictionary<string, bool>()
                {
                    {"GnMa1", false},
                    {"GnMa2", false},
                    {"GnMa3", false},
                    {"GnMa4", false},
                    {"GnMa5", false},
                    {"GnMa6", false},
                    {"GnMa7", false},
                    {"GnMa8", false},
                    {"GnMa9", false},
                    {"GnMa10", false}
                }
            },
            { Tuple.Create(Race.Gnome, Class.Rogue), new Dictionary<string, bool>()
                {
                    {"GnRo1", false},
                    {"GnRo2", false},
                    {"GnRo3", false},
                    {"GnRo4", false},
                    {"GnRo5", false},
                    {"GnRo6", false},
                    {"GnRo7", false},
                    {"GnRo8", false},
                    {"GnRo9", false},
                    {"GnRo10", false}
                }
            },
            { Tuple.Create(Race.Gnome, Class.Warlock), new Dictionary<string, bool>()
                {
                    {"GnWl1", false},
                    {"GnWl2", false},
                    {"GnWl3", false},
                    {"GnWl4", false},
                    {"GnWl5", false},
                    {"GnWl6", false},
                    {"GnWl7", false},
                    {"GnWl8", false},
                    {"GnWl9", false},
                    {"GnWl10", false}
                }
            },

            { Tuple.Create(Race.Orc, Class.Warrior), new Dictionary<string, bool>()
                {
                    {"OrWr1", false},
                    {"OrWr2", false},
                    {"OrWr3", false},
                    {"OrWr4", false},
                    {"OrWr5", false},
                    {"OrWr6", false},
                    {"OrWr7", false},
                    {"OrWr8", false},
                    {"OrWr9", false},
                    {"OrWr10", false}
                }
            },
            { Tuple.Create(Race.Orc, Class.Hunter), new Dictionary<string, bool>()
                {
                    {"OrHu1", false},
                    {"OrHu2", false},
                    {"OrHu3", false},
                    {"OrHu4", false},
                    {"OrHu5", false},
                    {"OrHu6", false},
                    {"OrHu7", false},
                    {"OrHu8", false},
                    {"OrHu9", false},
                    {"OrHu10", false}
                }
            },
            { Tuple.Create(Race.Orc, Class.Rogue), new Dictionary<string, bool>()
                {
                    {"OrRo1", false},
                    {"OrRo2", false},
                    {"OrRo3", false},
                    {"OrRo4", false},
                    {"OrRo5", false},
                    {"OrRo6", false},
                    {"OrRo7", false},
                    {"OrRo8", false},
                    {"OrRo9", false},
                    {"OrRo10", false}
                }
            },
            { Tuple.Create(Race.Orc, Class.Shaman), new Dictionary<string, bool>()
                {
                    {"OrSh1", false},
                    {"OrSh2", false},
                    {"OrSh3", false},
                    {"OrSh4", false},
                    {"OrSh5", false},
                    {"OrSh6", false},
                    {"OrSh7", false},
                    {"OrSh8", false},
                    {"OrSh9", false},
                    {"OrSh10", false}
                }
            },
            { Tuple.Create(Race.Orc, Class.Warlock), new Dictionary<string, bool>()
                {
                    {"OrWl1", false},
                    {"OrWl2", false},
                    {"OrWl3", false},
                    {"OrWl4", false},
                    {"OrWl5", false},
                    {"OrWl6", false},
                    {"OrWl7", false},
                    {"OrWl8", false},
                    {"OrWl9", false},
                    {"OrWl10", false}
                }
            },

            { Tuple.Create(Race.Undead, Class.Warrior), new Dictionary<string, bool>()
                {
                    {"UdWr1", false},
                    {"UdWr2", false},
                    {"UdWr3", false},
                    {"UdWr4", false},
                    {"UdWr5", false},
                    {"UdWr6", false},
                    {"UdWr7", false},
                    {"UdWr8", false},
                    {"UdWr9", false},
                    {"UdWr10", false}
                }
            },
            { Tuple.Create(Race.Undead, Class.Rogue), new Dictionary<string, bool>()
                {
                    {"UdRo1", false},
                    {"UdRo2", false},
                    {"UdRo3", false},
                    {"UdRo4", false},
                    {"UdRo5", false},
                    {"UdRo6", false},
                    {"UdRo7", false},
                    {"UdRo8", false},
                    {"UdRo9", false},
                    {"UdRo10", false}
                }
            },
            { Tuple.Create(Race.Undead, Class.Priest), new Dictionary<string, bool>()
                {
                    {"UdPr1", false},
                    {"UdPr2", false},
                    {"UdPr3", false},
                    {"UdPr4", false},
                    {"UdPr5", false},
                    {"UdPr6", false},
                    {"UdPr7", false},
                    {"UdPr8", false},
                    {"UdPr9", false},
                    {"UdPr10", false}
                }
            },
            { Tuple.Create(Race.Undead, Class.Mage), new Dictionary<string, bool>()
                {
                    {"UdMa1", false},
                    {"UdMa2", false},
                    {"UdMa3", false},
                    {"UdMa4", false},
                    {"UdMa5", false},
                    {"UdMa6", false},
                    {"UdMa7", false},
                    {"UdMa8", false},
                    {"UdMa9", false},
                    {"UdMa10", false}
                }
            },
            { Tuple.Create(Race.Undead, Class.Warlock), new Dictionary<string, bool>()
                {
                    {"UdWl1", false},
                    {"UdWl2", false},
                    {"UdWl3", false},
                    {"UdWl4", false},
                    {"UdWl5", false},
                    {"UdWl6", false},
                    {"UdWl7", false},
                    {"UdWl8", false},
                    {"UdWl9", false},
                    {"UdWl10", false}
                }
            },

            { Tuple.Create(Race.Tauren, Class.Warrior), new Dictionary<string, bool>()
                {
                    {"TaWr1", false},
                    {"TaWr2", false},
                    {"TaWr3", false},
                    {"TaWr4", false},
                    {"TaWr5", false},
                    {"TaWr6", false},
                    {"TaWr7", false},
                    {"TaWr8", false},
                    {"TaWr9", false},
                    {"TaWr10", false}
                }
            },
            { Tuple.Create(Race.Tauren, Class.Hunter), new Dictionary<string, bool>()
                {
                    {"TaHu1", false},
                    {"TaHu2", false},
                    {"TaHu3", false},
                    {"TaHu4", false},
                    {"TaHu5", false},
                    {"TaHu6", false},
                    {"TaHu7", false},
                    {"TaHu8", false},
                    {"TaHu9", false},
                    {"TaHu10", false}
                }
            },
            { Tuple.Create(Race.Tauren, Class.Shaman), new Dictionary<string, bool>()
                {
                    {"TaSh1", false},
                    {"TaSh2", false},
                    {"TaSh3", false},
                    {"TaSh4", false},
                    {"TaSh5", false},
                    {"TaSh6", false},
                    {"TaSh7", false},
                    {"TaSh8", false},
                    {"TaSh9", false},
                    {"TaSh10", false}
                }
            },
            { Tuple.Create(Race.Tauren, Class.Druid), new Dictionary<string, bool>()
                {
                    {"TaDr1", false},
                    {"TaDr2", false},
                    {"TaDr3", false},
                    {"TaDr4", false},
                    {"TaDr5", false},
                    {"TaDr6", false},
                    {"TaDr7", false},
                    {"TaDr8", false},
                    {"TaDr9", false},
                    {"TaDr10", false}
                }
            },
            { Tuple.Create(Race.Troll, Class.Warrior), new Dictionary<string, bool>()
                {
                    {"TrWr1", false},
                    {"TrWr2", false},
                    {"TrWr3", false},
                    {"TrWr4", false},
                    {"TrWr5", false},
                    {"TrWr6", false},
                    {"TrWr7", false},
                    {"TrWr8", false},
                    {"TrWr9", false},
                    {"TrWr10", false}
                }
            },
            { Tuple.Create(Race.Troll, Class.Hunter), new Dictionary<string, bool>()
                {
                    {"TrHu1", false},
                    {"TrHu2", false},
                    {"TrHu3", false},
                    {"TrHu4", false},
                    {"TrHu5", false},
                    {"TrHu6", false},
                    {"TrHu7", false},
                    {"TrHu8", false},
                    {"TrHu9", false},
                    {"TrHu10", false}
                }
            },
            { Tuple.Create(Race.Troll, Class.Rogue), new Dictionary<string, bool>()
                {
                    {"TrRo1", false},
                    {"TrRo2", false},
                    {"TrRo3", false},
                    {"TrRo4", false},
                    {"TrRo5", false},
                    {"TrRo6", false},
                    {"TrRo7", false},
                    {"TrRo8", false},
                    {"TrRo9", false},
                    {"TrRo10", false}
                }
            },
            { Tuple.Create(Race.Troll, Class.Priest), new Dictionary<string, bool>()
                {
                    {"TrPr1", false},
                    {"TrPr2", false},
                    {"TrPr3", false},
                    {"TrPr4", false},
                    {"TrPr5", false},
                    {"TrPr6", false},
                    {"TrPr7", false},
                    {"TrPr8", false},
                    {"TrPr9", false},
                    {"TrPr10", false}
                }
            },
            { Tuple.Create(Race.Troll, Class.Shaman), new Dictionary<string, bool>()
                {
                    {"TrSh1", false},
                    {"TrSh2", false},
                    {"TrSh3", false},
                    {"TrSh4", false},
                    {"TrSh5", false},
                    {"TrSh6", false},
                    {"TrSh7", false},
                    {"TrSh8", false},
                    {"TrSh9", false},
                    {"TrSh10", false}
                }
            },
            { Tuple.Create(Race.Troll, Class.Mage), new Dictionary<string, bool>()
                {
                    {"TrMa1", false},
                    {"TrMa2", false},
                    {"TrMa3", false},
                    {"TrMa4", false},
                    {"TrMa5", false},
                    {"TrMa6", false},
                    {"TrMa7", false},
                    {"TrMa8", false},
                    {"TrMa9", false},
                    {"TrMa10", false}
                }
            },
        };
    }
}
