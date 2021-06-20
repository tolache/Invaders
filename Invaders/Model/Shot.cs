using System;
using System.Drawing;

namespace Invaders.Model
{
    public class Shot : MovingBody
    {
        private const double ShotSpeed = 95;

        public static Size ShotSize = new(2, 10);
        public Direction Direction { get; private set; }

        public Shot(Point location, Direction direction) : base(location, ShotSize)
        {
            Speed = ShotSpeed;
            Direction = direction;
            LastMoved = DateTime.Now;
        }

        public void Move()
        {
            Vector vector = new (Direction, Speed);
            Move(vector);
        }
    }
}