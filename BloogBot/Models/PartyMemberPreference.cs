using BloogBot.Game.Enums;
using BloogBot.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloogBot.Models
{
    public class PartyMemberPreference
    {
        public Race Race { get; set; }
        public Class Class { get; set; }
        public Role Role { get; set; }
    }
}
