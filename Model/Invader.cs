using System;
using System.Drawing;

namespace Invaders.Model
{
    public class Invader : Ship
    {
        public static Size InvaderSize => new(15, 15);
        public InvaderType Type { get; }
        public int Score { get; }
        
        public Invader(InvaderType type, Point location, Size size) : base(location, size)
        {
            Type = type;
            switch (Type)
            {
                case InvaderType.Star:
                    Score = 10;
                    break;
                case InvaderType.Satellite:
                    Score = 20;
                    break;
                case InvaderType.Saucer:
                    Score = 30;
                    break;
                case InvaderType.Bug:
                    Score = 40;
                    break;
                case InvaderType.Spaceship:
                    Score = 50;
                    break;
                default:
                    throw new NotImplementedException($"Invader type '{type}' is not implemented.");
            }
        }

        public override void Move(Direction direction)
        {
            Point oldLocation = Location;
            switch (direction)
            {
                case Direction.Left:
                    Location = new Point(oldLocation.X - Size.Width, oldLocation.Y);
                    break;
                case Direction.Right:
                    Location = new Point(oldLocation.X + Size.Width, oldLocation.Y);
                    break;
                case Direction.Down:
                    Location = new Point(oldLocation.X, oldLocation.Y + Size.Height);
                    break;
                default:
                    throw new NotImplementedException($"Moving {GetType()} {direction} is not implemented.");
            }
        }
    }
}