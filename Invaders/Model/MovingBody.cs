using System.Drawing;

namespace Invaders.Model
{
    public abstract class MovingBody : Mover
    {
        public Size Size { get; }
        public Rectangle Area => new(Location, Size);
        public int MoveDistance { get; protected init; }

        protected MovingBody(Point location, Size size)
        {
            Location = location;
            Size = size;
        }
    }
}