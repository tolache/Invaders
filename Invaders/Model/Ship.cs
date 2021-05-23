using System.Drawing;

namespace Invaders.Model
{
    public abstract class Ship
    {
        public Point Location { get; protected set; }
        public Size Size { get; private set; }
        public Rectangle Area => new Rectangle(Location, Size);
        public ShipStatus ShipStatus;

        protected Ship(Point location, Size size)
        {
            Location = location;
            Size = size;
            ShipStatus = ShipStatus.AliveNormal;
        }

        public abstract void Move(Direction direction);
    }
    
    public enum ShipStatus
    {
        AliveNormal,
        AliveDiveBombing,
        Killed,
        OffScreen,
    }
}