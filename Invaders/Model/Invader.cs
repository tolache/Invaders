using System;
using System.Drawing;

namespace Invaders.Model
{
    public class Invader : Ship
    {
        public static Size InvaderSize => new(15, 15);
        public static Size MothershipSize => new(InvaderSize.Width * 5, InvaderSize.Height * 2);

        private const int InvaderSpeed = 3; 
        
        public InvaderType Type { get; }
        public int Score { get; private set; }

        public Invader(InvaderType type, Point location, Size size) : base(location, size)
        {
            Type = type;
            SetScoreValue(type);
            Speed = InvaderSpeed;
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
                case Direction.Down:
                    Location = new Point(oldLocation.X, oldLocation.Y + InvaderSize.Height);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), $"Unexpected direction '{direction}' for invader type '{Type}'.");
            }
        }

        private void SetScoreValue(InvaderType type)
        {
            switch (type)
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
                case InvaderType.WatchIt:
                    Score = 60;
                    break;
                case InvaderType.Mothership:
                    Score = 250;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), $"Unexpected invader type: {type}.");
            }
        }
    }
}