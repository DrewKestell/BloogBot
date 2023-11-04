using RaidMemberBot.Game.Statics;
using System;

namespace RaidMemberBot.Game.Frames.FrameObjects
{
    /// <summary>
    ///     Representing a point on the flight map we can travel to
    /// </summary>
    public abstract class TaxiNode
    {
        internal TaxiNode(int parNodeNumber)
        {
            NodeNumber = parNodeNumber;
            string[] result = Lua.Instance.ExecuteWithResult(
                $"{{0}} = TaxiNodeCost({parNodeNumber}) {{1}} = TaxiNodeName({parNodeNumber}) {{2}} = TaxiNodeGetType({parNodeNumber})");
            Cost = Convert.ToInt32(result[0]);
            Name = result[1];
            Status = result[2];
        }

        /// <summary>
        ///     Status like Reachable, Current etc.
        /// </summary>
        /// <value>
        ///     The status.
        /// </value>
        public string Status { get; }

        /// <summary>
        ///     The name of the taxi node (eg: Stormwind)
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        ///     The cost in copper to take the taxi
        /// </summary>
        /// <value>
        ///     The cost.
        /// </value>
        public int Cost { get; private set; }

        /// <summary>
        ///     The number of the node
        /// </summary>
        /// <value>
        ///     The node number.
        /// </value>
        public int NodeNumber { get; }
    }
}
