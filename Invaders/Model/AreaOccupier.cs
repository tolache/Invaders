using System.Drawing;

namespace Invaders.Model
{
    public abstract class AreaOccupier
    {
        public Point Location { get; protected set; }
        public Size Size { get; }
        public Rectangle Area => new(Location, Size);

        protected AreaOccupier(Point location, Size size)
        {
            Location = location;
            Size = size;
        }
    }
}