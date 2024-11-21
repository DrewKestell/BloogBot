namespace BotRunner.Frames
{
    public interface ITaxiFrame
    {
        bool IsOpen { get; }
        void Close(); 
        List<TaxiNode> Nodes { get; }
        int NodesAvailable { get; }
        string CurrentNodeName { get; }
        void SelectNodeByNumber(int parNodeNumber);
        void SelectNodeByName(string parNodeName);
        bool HasNodeUnlocked(int nodeId);
        void SelectNode(int nodeId);
    }

    /// <summary>
    ///     Representing a point on the flight map we can travel to
    /// </summary>
    public abstract class TaxiNode
    {
        /// <summary>
        ///     Status like Reachable, Current etc.
        /// </summary>
        /// <value>
        ///     The status.
        /// </value>
        public string Status { get; } = string.Empty;

        /// <summary>
        ///     The name of the taxi node (eg: Stormwind)
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; } = string.Empty;

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
