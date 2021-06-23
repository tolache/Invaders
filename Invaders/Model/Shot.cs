using System.Drawing;

namespace Invaders.Model
{
    public class Shot : MovingBody
    {
        private const int ShotSpeed = 6;

        public static Size ShotSize = new(2, 10);
        public Direction Direction { get; private set; }

        public Shot(Point location, Direction direction) : base(location, ShotSize)
        {
            MoveDistance = ShotSpeed;
            Direction = direction;
        }

        public void Move()
        {
            Move(Direction, MoveDistance);
        }
    }
}