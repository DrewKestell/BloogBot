namespace BloogBot.AI
{
    public static class AccountHelper
    {
        public static string GetAccountByRaceAndClass(string race, string clazz) {

            switch (race)
            {
                case "Human":
                    switch (clazz)
                    {
                        case "Mage":
                            return "HuMa1";
                        case "Paladin":
                            return "HuPa1";
                        case "Priest":
                            return "HuPr1";
                        case "Rogue":
                            return "HuRo1";
                        case "Warlock":
                            return "HuWl1";
                        case "Warrior":
                            return "HuWr1";
                    }
                    break;
                case "Dwarf":
                    switch (clazz)
                    {
                        case "Hunter":
                            return "DwHu1";
                        case "Paladin":
                            return "DwPa1";
                        case "Priest":
                            return "DwPr1";
                        case "Rogue":
                            return "DwRo1";
                        case "Warrior":
                            return "DwWr1";
                    }
                    break;
                case "Night Elf":
                    switch (clazz)
                    {
                        case "Druid":
                            return "NEDr1";
                        case "Hunter":
                            return "NEHu1";
                        case "Priest":
                            return "NEPr1";
                        case "Rogue":
                            return "NERo1";
                        case "Warrior":
                            return "NEWr1";
                    }
                    break;
                case "Gnome":
                    switch (clazz)
                    {
                        case "Mage":
                            return "GnMa1";
                        case "Rogue":
                            return "GnRo1";
                        case "Warlock":
                            return "GnWl1";
                        case "Warrior":
                            return "GnWr1";
                    }
                    break;
                case "Orc":
                    switch (clazz)
                    {
                        case "Hunter":
                            return "OrHu1";
                        case "Rogue":
                            return "OrRo1";
                        case "Shaman":
                            return "OrSh1";
                        case "Warlock":
                            return "OrWl1";
                        case "Warrior":
                            return "OrWr1";
                    }
                    break;
                case "Undead":
                    switch (clazz)
                    {
                        case "Mage":
                            return "UdMa1";
                        case "Priest":
                            return "UdPr1";
                        case "Rogue":
                            return "UdRo1";
                        case "Warlock":
                            return "UdWl1";
                        case "Warrior":
                            return "UdWr1";
                    }
                    break;
                case "Tauren":
                    switch (clazz)
                    {
                        case "Druid":
                            return "TaDr1";
                        case "Hunter":
                            return "TaHu1";
                        case "Shaman":
                            return "TaSh1";
                        case "Warrior":
                            return "TaWr1";
                    }
                    break;
                case "Troll":
                    switch (clazz)
                    {
                        case "Hunter":
                            return "TrHu1";
                        case "Mage":
                            return "TrMa1";
                        case "Priest":
                            return "TrPr1";
                        case "Rogue":
                            return "TrRo1";
                        case "Shaman":
                            return "TrSh1";
                        case "Warrior":
                            return "TrWr1";
                    }
                    break;
            }
            return "TrWr1";
        }
    }
}
