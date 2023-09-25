using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BloogBot.Models.Dto
{
    public class InstanceCommand
    {
        public static readonly string LOGIN = "LOGIN";
        public static readonly string SET = "SET";
        public static readonly string INVITE = "INVITE";
        public static readonly string JOIN = "JOIN";
        public static readonly string PULL = "PULL";
        public string StateName { get; set; }
        public string CommandParam1 { get; set; }
        public string CommandParam2 { get; set; }
        public string CommandParam3 { get; set; }
        public string CommandParam4 { get; set; }
    }
}
