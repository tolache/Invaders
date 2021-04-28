using System;

namespace Invaders.Model
{
    public class ShipChangedEventArgs : EventArgs
    {
        public Ship Ship { get; private set; }
        public bool Killed { get; private set; }

        public ShipChangedEventArgs(Ship ship, bool killed)
        {
            Ship = ship;
            Killed = killed;
        }
    }
}