using System;
using System.Drawing;

namespace Invaders.Model
{
    public class Shot
    {
        public const double Speed = 95;

        public static Size ShotSize = new Size(2, 10);
        public Point Location { get; private set; }
        public Rectangle Area => new Rectangle(Location, ShotSize);

        public Direction Direction { get; private set; }
        
        private DateTime _lastMoved;

        public Shot(Point location, Direction direction)
        {
            Location = location;
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