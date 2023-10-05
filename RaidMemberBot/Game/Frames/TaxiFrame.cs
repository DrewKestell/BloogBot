using RaidMemberBot.Game.Frames.FrameObjects;
using RaidMemberBot.Game.Statics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.Game.Frames
{
    /// <summary>
    ///     Represents a Taxi-Frame
    /// </summary>
    public sealed class TaxiFrame
    {
        private static volatile TaxiFrame _instance;
        private static readonly object lockObject = new object();
        private static volatile bool _isOpen;
        private static volatile bool _abort;


        private readonly List<TaxiNode> _Nodes = new List<TaxiNode>();

        private TaxiFrame()
        {
            var result = Lua.Instance.ExecuteWithResult("{0} = NumTaxiNodes()");
            NodesAvailable = Convert.ToInt32(result[0]);
            _Nodes.Clear();
            for (var i = 0; i < NodesAvailable; i++)
            {
                var tni = new TaxiNodeInterface(i + 1);
                if (tni.Status == "NONE") continue;
                _Nodes.Add(tni);
                if (tni.Status == "CURRENT")
                    CurrentNodeName = tni.Name;
            }
            NodesAvailable = _Nodes.Count;
        }

        /// <summary>
        ///     Access to the currently open frame
        /// </summary>
        /// <value>
        ///     The frame.
        /// </value>
        public static TaxiFrame Instance => _instance;

        /// <summary>
        ///     Tells whether a Taxi-Frame is open or not
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is open; otherwise, <c>false</c>.
        /// </value>
        public static bool IsOpen => _isOpen;

        /// <summary>
        ///     A list of all Taxi-Nodes avaible in the current Taxi-Frame
        /// </summary>
        /// <value>
        ///     The nodes.
        /// </value>
        public IReadOnlyList<TaxiNode> Nodes => _Nodes;

        /// <summary>
        ///     Number of all Nodes avaible for current Taxi-Frame
        /// </summary>
        /// <value>
        ///     The nodes avaible.
        /// </value>
        public int NodesAvailable { get; }

        /// <summary>
        ///     The name of the current Node
        /// </summary>
        /// <value>
        ///     The name of the current node.
        /// </value>
        public string CurrentNodeName { get; private set; }

        internal static void Create()
        {
            lock (lockObject)
            {
                _isOpen = false;
                _abort = false;

                var tmp = new TaxiFrame();
                if (_abort) return;
                _instance = tmp;
                _isOpen = true;
            }
        }

        internal static void Destroy()
        {
            _abort = true;
            lock (lockObject)
            {
                _isOpen = false;
            }
        }

        /// <summary>
        ///     Take a taxi to the node selected by number
        /// </summary>
        /// <param name="parNodeNumber">The node number.</param>
        public void SelectNodeByNumber(int parNodeNumber)
        {
            if (Nodes.All(x => x.NodeNumber != parNodeNumber)) return;
            Lua.Instance.Execute("TakeTaxiNode(" + parNodeNumber + ")");
        }

        /// <summary>
        ///     Take a taxi to the node selected by name
        /// </summary>
        /// <param name="parNodeName">Name of the node.</param>
        public void SelectNodeByName(string parNodeName)
        {
            foreach (var x in Nodes)
                if (x.Name == parNodeName)
                {
                    Lua.Instance.Execute("TakeTaxiNode(" + x.NodeNumber + ")");
                    return;
                }
        }

        private class TaxiNodeInterface : TaxiNode
        {
            internal TaxiNodeInterface(int parNodeNumber) : base(parNodeNumber)
            {
            }
        }
    }
}
