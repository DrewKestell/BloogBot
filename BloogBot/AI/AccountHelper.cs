using BloogBot.Game.Enums;
using System;

namespace BloogBot.AI
{
    public static class AccountHelper
    {
        public static string GetAccountByRaceAndClass(Race race, Class clazz) {
            Console.WriteLine($"Race: {race} Class: {clazz}");
            switch (race)
            {
                case Race.Human:
                    switch (clazz)
                    {
                        case Class.Mage:
                            return "HuMa1";
                        case Class.Paladin:
                            return "HuPa1";
                        case Class.Priest:
                            return "HuPr1";
                        case Class.Rogue:
                            return "HuRo1";
                        case Class.Warlock:
                            return "HuWl1";
                        case Class.Warrior:
                            return "HuWr1";
                    }
                    break;
                case Race.Dwarf:
                    switch (clazz)
                    {
                        case Class.Hunter:
                            return "DwHu1";
                        case Class.Paladin:
                            return "DwPa1";
                        case Class.Priest:
                            return "DwPr1";
                        case Class.Rogue:
                            return "DwRo1";
                        case Class.Warrior:
                            return "DwWr1";
                    }
                    break;
                case Race.Nightelf:
                    switch (clazz)
                    {
                        case Class.Druid:
                            return "NEDr1";
                        case Class.Hunter:
                            return "NEHu1";
                        case Class.Priest:
                            return "NEPr1";
                        case Class.Rogue:
                            return "NERo1";
                        case Class.Warrior:
                            return "NEWr1";
                    }
                    break;
                case Race.Gnome:
                    switch (clazz)
                    {
                        case Class.Mage:
                            return "GnMa1";
                        case Class.Rogue:
                            return "GnRo1";
                        case Class.Warlock:
                            return "GnWl1";
                        case Class.Warrior:
                            return "GnWr1";
                    }
                    break;
                case Race.Orc:
                    switch (clazz)
                    {
                        case Class.Hunter:
                            return "OrHu1";
                        case Class.Rogue:
                            return "OrRo1";
                        case Class.Shaman:
                            return "OrSh1";
                        case Class.Warlock:
                            return "OrWl1";
                        case Class.Warrior:
                            return "OrWr1";
                    }
                    break;
                case Race.Undead:
                    switch (clazz)
                    {
                        case Class.Mage:
                            return "UdMa1";
                        case Class.Priest:
                            return "UdPr1";
                        case Class.Rogue:
                            return "UdRo1";
                        case Class.Warlock:
                            return "UdWl1";
                        case Class.Warrior:
                            return "UdWr1";
                    }
                    break;
                case Race.Tauren:
                    switch (clazz)
                    {
                        case Class.Druid:
                            return "TaDr1";
                        case Class.Hunter:
                            return "TaHu1";
                        case Class.Shaman:
                            return "TaSh1";
                        case Class.Warrior:
                            return "TaWr1";
                    }
                    break;
                case Race.Troll:
                    switch (clazz)
                    {
                        case Class.Hunter:
                            return "TrHu1";
                        case Class.Mage:
                            return "TrMa1";
                        case Class.Priest:
                            return "TrPr1";
                        case Class.Rogue:
                            return "TrRo1";
                        case Class.Shaman:
                            return "TrSh1";
                        case Class.Warrior:
                            return "TrWr1";
                    }
                    break;
            }
            return "TrWr1";
        }
    }
}
