using System;
using System.Drawing;

namespace Invaders.Model
{
    public class Shot : AreaOccupier
    {
        private const double Speed = 95;
        private DateTime _lastMoved;

        public static Size ShotSize = new(2, 10);
        public Direction Direction { get; }

        public Shot(Point location, Direction direction) : base(location, ShotSize)
        {
            Direction = direction;
            _lastMoved = DateTime.Now;
        }

        public void Move()
        {
            TimeSpan timeSinceLastMoved = DateTime.Now - _lastMoved;
            double distance = timeSinceLastMoved.Milliseconds * Speed / 1000;
            if (Direction == Direction.Up) distance *= -1;
            Location = new Point(Location.X, (int) (Location.Y + distance));
            _lastMoved = DateTime.Now;
        }
    }
}