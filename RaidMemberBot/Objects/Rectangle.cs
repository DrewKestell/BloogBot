using System;
using System.Collections.Generic;

namespace RaidMemberBot.Objects
{
    internal class Rectangle
    {
        private readonly List<Location> _points;
        private readonly float _xLength;
        private readonly float _yLength;
        private readonly Random _random;
        private float Percent => _random.Next(30, 70) / (float)100;

        internal bool IsInRectangle(Location loc)
        {
            float minX = _points[0].X;
            float maxX = minX + _xLength;
            if (minX > maxX)
            {
                (maxX, minX) = (minX, maxX);
            }
            float minY = _points[0].Y;
            float maxY = minY + _yLength;
            // ReSharper disable once InvertIf
            if (minY > maxY)
            {
                (maxY, minY) = (minY, maxY);
            }
            return loc.X >= minX && loc.X <= maxX && loc.Y >= minY && loc.Y <= maxY;
        }

        internal Location GetLocInRectangle()
        {
            return new Location(_points[0].X + _xLength * Percent, _points[0].Y + _yLength * Percent, _points[0].Z);
        }

        internal Rectangle(Location p1, Location p2)
        {
            _random = new Random();
            float xLength = p1.X - p2.X;
            float yLength = p1.Y - p2.Y;
            Location newP2 = new Location(p1.X + (p1.X > p2.X ? -xLength : xLength), p1.Y);
            Location p3 = new Location(newP2.X, newP2.Y + (p2.Y > p1.Y ? -yLength : yLength));
            Location p4 = new Location(p3.X + (p1.X > p2.X ? xLength : -xLength), p3.Y);
            _points = new List<Location> { p1, newP2, p3, p4 };

            float startX = _points[0].X;
            float startY = _points[0].Y;
            for (int i = 1; i < 4; i++)
            {
                Location item = _points[i];
                if (item.X != startX)
                    _xLength = item.X - startX;
                if (item.Y != startY)
                    _yLength = item.Y - startY;
            }
        }
    }
}
