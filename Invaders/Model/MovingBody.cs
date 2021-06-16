using System.Drawing;

namespace Invaders.Model
{
    public abstract class MovingBody
    {
        public Point Location { get; protected set; }
        public Size Size { get; }
        public Rectangle Area => new(Location, Size);

        protected MovingBody(Point location, Size size)
        {
            Location = location;
            Size = size;
        }

        public abstract void Move(Direction direction);
    }
}