using System;
using System.Drawing;

namespace Invaders.Model
{
    public class Invader : Ship
    {
        public static Size InvaderSize => new(15, 15);
        public static Size MothershipSize => new(InvaderSize.Width * 5, InvaderSize.Height * 2);

        public const double InvaderSpeed = 17.5;
        public const double InvaderFormationDownSpeed = 500;
        
        public InvaderType Type { get; }
        public int Score { get; private set; }

        public Invader(InvaderType type, Point location, Size size) : base(location, size)
        {
            Type = type;
            SetScoreValue(type);
            Speed = InvaderSpeed;
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