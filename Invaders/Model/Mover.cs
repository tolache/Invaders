using System;
using System.Drawing;

namespace Invaders.Model
{
    public abstract class Mover
    {
        public Point Location { get; protected set; }
        
        public void Move(Direction direction, int distance)
        {
            distance = Math.Abs(distance);
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