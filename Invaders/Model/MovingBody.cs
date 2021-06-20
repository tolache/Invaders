using System;
using System.Drawing;
using static System.Math;

namespace Invaders.Model
{
    public abstract class MovingBody
    {
        protected DateTime LastMoved = DateTime.MinValue;
        
        public Point Location { get; private set; }
        public Size Size { get; }
        public Rectangle Area => new(Location, Size);
        public double Speed { get; protected init; }

        protected MovingBody(Point location, Size size)
        {
            Location = location;
            Size = size;
        }

        public virtual void Move(Vector vector)
        {
            TimeSpan timeSinceLastMoved = DateTime.Now - LastMoved;
            int distance = (int) Round(timeSinceLastMoved.Milliseconds * vector.Speed / 1000, MidpointRounding.AwayFromZero);
            ChangeLocation(vector.Direction, distance);
            LastMoved = DateTime.Now;
        }

        protected void ChangeLocation(Direction direction, int distance)
        {
            Location = direction switch
            {
                Direction.Up => new Point(Location.X, Location.Y - distance),
                Direction.Down => new Point(Location.X, Location.Y + distance),
                Direction.Left => new Point(Location.X - distance, Location.Y),
                Direction.Right => new Point(Location.X + distance, Location.Y),
                _ => Location
            };
        }
    }
}