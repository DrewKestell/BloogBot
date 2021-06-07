using System;
using System.Collections.Generic;

namespace BloogBot
{
    public class Probe
    {
        public Probe(Action callback, Action killswitch)
        {
            Callback = callback;
            Killswitch = killswitch;
        }

        public Action Callback { get; }

        public Action Killswitch { get; }

        public string CurrentState { get; set; }

        public string CurrentPosition { get; set; }

        public string CurrentZone { get; set; }

        public string TargetName { get; set; }

        public string TargetClass { get; set; }

        public string TargetCreatureType { get; set; }

        public string TargetPosition { get; set; }

        public string TargetRange { get; set; }

        public string TargetFactionId { get; set; }

        public string TargetIsCasting { get; set; }

        public string TargetIsChanneling { get; set; }

        public IList<ulong> BlacklistedMobIds { get; set; }
    }
}
