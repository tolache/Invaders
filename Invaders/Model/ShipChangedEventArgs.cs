using System;

namespace Invaders.Model
{
    public class ShipChangedEventArgs : EventArgs
    {
        public Ship Ship { get; private set; }

        public ShipChangedEventArgs(Ship ship)
        {
            Ship = ship;
        }
    }
}