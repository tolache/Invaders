using System.Drawing;

namespace Invaders.Model
{
    public abstract class Ship : MovingBody
    {
        public ShipStatus ShipStatus;

        protected Ship(Point location, Size size) : base(location, size)
        {
            ShipStatus = ShipStatus.Alive;
        }
    }

    public enum ShipStatus
    {
        Alive,
        Killed,
        OffScreen,
    }
}