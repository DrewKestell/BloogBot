using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidMemberBot.Models.Dto
{
    public class ConfigurationResponse
    {
        public int NavigationServerPort { get; set; }
        public int DatabaseServerPort { get; set; }
        public int RaidLeaderServerPort { get; set; }
    }
}
