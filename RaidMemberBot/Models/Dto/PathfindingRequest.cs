using System.Numerics;

namespace RaidMemberBot.Models.Dto
{
    public class PathfindingRequest
    {
        public bool IsRaidLeader { get; set; }
        public uint MapId { get; set; }
        public Vector3 StartLocation { get; set; }
        public Vector3 EndLocation { get; set; }
        public bool SmoothPath { get; set; }
    }
}
