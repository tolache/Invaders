using System.Drawing;

namespace Invaders.Model
{
    public abstract class Ship : MovingBody
    {
        public ShipStatus ShipStatus;
        
        public int Speed { get; protected init; }

        protected Ship(Point location, Size size) : base(location, size)
        {
            ShipStatus = ShipStatus.AliveNormal;
        }
    }

    public enum ShipStatus
    {
        AliveNormal,
        AliveDiveBombing,
        Killed,
        OffScreen,
    }
}