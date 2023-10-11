using RaidMemberBot.Constants;
using RaidMemberBot.Game.Statics;
using System;

namespace RaidMemberBot.Objects
{
    /// <summary>
    ///     Representing an ingame 3D vector
    /// </summary>
    public class Location
    {
        private float _X;
        private float _Y;
        private float _Z;

        internal _XYZ ToStruct;

        public Location(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            ToStruct = new _XYZ(X, Y, Z);
        }

        public Location()
        {
            X = 0;
            Y = 0;
            Z = 0;
            ToStruct = new _XYZ(X, Y, Z);
        }

        internal Location(_XYZ parStruct)
        {
            X = parStruct.X;
            Y = parStruct.Y;
            Z = parStruct.Z;
            ToStruct = parStruct;
        }

        internal Location(ref _XYZ parStruct)
        {
            X = parStruct.X;
            Y = parStruct.Y;
            Z = parStruct.Z;
            ToStruct = parStruct;
        }

        public Location(float x, float y)
        {
            X = x;
            Y = y;
            Z = 0;
            ToStruct = new _XYZ(X, Y, Z);
        }

        /// <summary>
        ///     X-coordinate
        /// </summary>
        public float X
        {
            get { return _X; }
            internal set
            {
                _X = value;
                ToStruct.X = value;
            }
        }

        /// <summary>
        ///     Y-coordinate
        /// </summary>
        public float Y
        {
            get { return _Y; }
            internal set
            {
                _Y = value;
                ToStruct.Y = value;
            }
        }

        /// <summary>
        ///     Z-coordinate
        /// </summary>
        public float Z
        {
            get { return _Z; }
            internal set
            {
                _Z = value;
                ToStruct.Z = value;
            }
        }
        public override string ToString()
        {
            return $"X: {X} Y: {Y} Z: {Z}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public float GetDistanceTo(Location location)
        {
            var deltaX = X - location.X;
            var deltaY = Y - location.Y;
            var deltaZ = Z - location.Z;
            return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        }

        /// <summary>
        /// Returns the distance to the x/y coordinate of the location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public float GetDistanceTo2D(Location location)
        {
            var deltaX = X - location.X;
            var deltaY = Y - location.Y;
            return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        /// <summary>
        /// Gets the distance to the current player
        /// </summary>
        /// <returns></returns>
        public float DistanceToPlayer() => GetDistanceTo(ObjectManager.Instance.Player.Location);

        /// <summary>
        /// Checks to see if the location is in range to the given location
        /// </summary>
        /// <param name="location"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool InRangeTo(Location location, float distance = 5f) => GetDistanceTo(location) <= distance;

        /// <summary>
        /// Checks to see if the location is in range to the given player
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool InRangeTo(WoWUnit unit, float distance = 5f) => InRangeTo(unit.Location, distance);

        /// <summary>
        /// Checks to see if the location is in range to the player
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool InRangeTo(float distance = 5f) => InRangeTo(ObjectManager.Instance.Player, distance);
    }
}
