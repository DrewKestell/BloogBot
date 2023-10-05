using RaidMemberBot.ExtensionMethods;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using FluentBehaviourTree;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.Game
{
    /// <summary>
    /// Aids the bot with taking transports
    /// </summary>
    public class Transport
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="waitingPoint">The point the character should wait for the transport arrival</param>
        /// <param name="transportStartLocation">The location of the transport when the bot can step on</param>
        /// <param name="transportEndlocation">The location of the transport when the bot can step off</param>
        /// <param name="onTransportMovingPoints">A list of waypoints the bot will walk after after entering the transport (it will stop on the last one and wait for arrival - must be relative to the transport)</param>
        /// <param name="endPoint">The point the bot should approach after the transport arrived at its destination</param>
        public Transport(Location waitingPoint, Location transportStartLocation, Location transportEndlocation, List<Location> onTransportMovingPoints, Location endPoint)
        {
            TransportWait = waitingPoint;
            TransportRestState = transportStartLocation;
            TransportArrivedState = transportEndlocation;
            TransportEnd = endPoint;
            TransportMovingPoints = onTransportMovingPoints;
            BuildBehaviorTree();
        }

        /// <summary>
        /// Evaluates which step it has to do next in the process of taking a transport
        /// </summary>
        /// <returns>True if the bot arrived at the destination point</returns>
        public bool Process()
        {
            if (!ObjectManager.Instance.IsIngame) return false;
            _behavior.Tick(new TimeData());
            return _arrivedAtEndPoint && _leftTransport;
        }

        internal void BuildBehaviorTree()
        {
            var builder = new BehaviourTreeBuilder();
            _behavior = builder.Sequence("Transport")
                .Selector("Transport")
                .Do("WalkToEndpoint", data =>
                {
                    if (!_leftTransport) return BehaviourTreeStatus.Failure;
                    var player = ObjectManager.Instance.Player;
                    var playerPos = player.Location;
                    var transport = player.CurrentTransport;
                    var loc = transport == null ? TransportEnd : TransportEnd.GetRelativeToPlayerTransport();
                    if (playerPos.GetDistanceTo(loc) >= 1.3f)
                    {
                        player.CtmTo(loc);
                        return BehaviourTreeStatus.Running;
                    }
                    _arrivedAtEndPoint = true;
                    return BehaviourTreeStatus.Success;
                })
                .Do("WaitForArrivalAndWalk", data =>
                {
                    if (!_onTransport) return BehaviourTreeStatus.Failure;
                    var player = ObjectManager.Instance.Player;
                    var transport = player.CurrentTransport;

                    var transportArrived = ObjectManager.Instance.GameObjects.FirstOrDefault(x => x.Location.GetDistanceTo(TransportArrivedState) <= 0.1f) != null;
                    if (transport == null)
                    {
                        if (transportArrived)
                            _leftTransport = true;
                        return BehaviourTreeStatus.Success;
                    }
                    if (transportArrived)
                    {
                        player.CtmTo(TransportEnd.GetRelativeToPlayerTransport());
                        return BehaviourTreeStatus.Running;
                    }
                    player.CtmStopMovement();
                    return BehaviourTreeStatus.Running;
                })
                .Do("WaitAndWalkOnTransport", data =>
                {
                    if (!_arrivedAtWaitPoint) return BehaviourTreeStatus.Failure;
                    var player = ObjectManager.Instance.Player;
                    var myTransport =
                        ObjectManager.Instance.GameObjects.FirstOrDefault(
                            x => x.Location.GetDistanceTo(TransportRestState) <= 0.1f);
                    Location loc;
                    if (player.TransportGuid == 0)
                    {
                        if (myTransport != null)
                        {
                            loc = TransportMovingPoints[0].GetAbsoluteFromRelativeTransportLocation(myTransport);
                            _movingIndex = 0;
                            player.CtmTo(loc);
                        }
                        else
                        {
                            player.CtmStopMovement();
                        }
                        return BehaviourTreeStatus.Running;
                    }
                    var playerPos = player.Location;
                    loc = TransportMovingPoints[_movingIndex];
                    if (playerPos.GetDistanceTo(loc) < 1.3)
                    {
                        if (_movingIndex >= TransportMovingPoints.Count - 1)
                        {
                            _onTransport = true;
                            return BehaviourTreeStatus.Success;
                        }
                        _movingIndex++;
                        loc = TransportMovingPoints[_movingIndex];
                    }
                    player.CtmTo(loc);
                    return BehaviourTreeStatus.Running;
                })
                .Do("GoToWaitingPoint", data =>
                {
                    var player = ObjectManager.Instance.Player;
                    if (player.CurrentTransport == null && !_onTransport && !_leftTransport)
                    {
                        var pos = player.Location;

                        if (pos.GetDistanceTo(TransportWait) >= 1.3f)
                        {
                            player.CtmTo(TransportWait);
                            return BehaviourTreeStatus.Running;
                        }
                        _arrivedAtWaitPoint = true;
                        return BehaviourTreeStatus.Success;
                    }
                    return BehaviourTreeStatus.Running;
                })
                .End()
                .Build();
        }

        private IBehaviourTreeNode _behavior;
        private bool _arrivedAtEndPoint;
        private bool _onTransport;
        private bool _arrivedAtWaitPoint;
        private bool _leftTransport;
        private int _movingIndex;

        internal Location TransportWait { get; set; }
        internal Location TransportEnd { get; set; }
        internal Location TransportRestState { get; set; }
        internal Location TransportArrivedState { get; set; }
        internal List<Location> TransportMovingPoints { get; set; }
    }
}
