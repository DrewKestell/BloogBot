using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloogBot.Models
{
    public class PartyMemberPreference
    {
		public string Race { get; set; }
        public string Class { get; set; }
        public bool IsTank { get; set; }
        public bool IsDamage { get; set; }
        public bool IsHealer { get; set; }
    }
}
