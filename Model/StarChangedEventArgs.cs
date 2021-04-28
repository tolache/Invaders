using System;
using System.Drawing;

namespace Invaders.Model
{
    public class StarChangedEventArgs : EventArgs
    {
        public Point Location { get; private set; }
        public bool Disappeared { get; private set; }

        public StarChangedEventArgs(Point location, bool disappeared)
        {
            Location = location;
            Disappeared = disappeared;
        }
    }
}