using System;

namespace Invaders.Model
{
    public class ShotMovedEventArgs : EventArgs
    {
        public Shot Shot { get; }
        public bool Disappeared { get; }

        public ShotMovedEventArgs(Shot shot, bool disappeared)
        {
            Shot = shot;
            Disappeared = disappeared;
        }
    }
}