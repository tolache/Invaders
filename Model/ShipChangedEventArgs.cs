using System;

namespace Invaders.Model
{
    public class ShipChangedEventArgs : EventArgs
    {
        public Ship Ship { get; private set; }
        public ShipStatus ShipStatus { get; private set; }

        public ShipChangedEventArgs(Ship ship, ShipStatus shipStatus)
        {
            Ship = ship;
            ShipStatus = shipStatus;
        }
    }

    public enum ShipStatus
    {
        Alive,
        Killed,
        OffScreen,
    }
}