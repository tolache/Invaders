using System;
using System.Drawing;

namespace Invaders.Model
{
    public class Player : Ship
    {
        public const int Speed = 3;
        public static readonly Size PlayerSize = new(25,15);

        public Player(Point location, Size size) : base(location, size)
        {
        }

        public override void Move(Direction direction)
        {
            Point oldLocation = Location;
            switch (direction)
            {
                case Direction.Left:
                    Location = new Point(oldLocation.X - Speed, oldLocation.Y);
                    break;
                case Direction.Right:
                    Location = new Point(oldLocation.X + Speed, oldLocation.Y);
                    break;
                default:
                    throw new NotImplementedException($"Moving {GetType()} {direction} is not implemented.");
            }
        }
    }
}