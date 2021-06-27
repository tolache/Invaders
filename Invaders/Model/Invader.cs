using System;
using System.Drawing;

namespace Invaders.Model
{
    public class Invader : Ship
    {
        public static Size InvaderSize => new(15, 15);
        public static Size MothershipSize => new(InvaderSize.Width * 5, InvaderSize.Height * 2);

        private const int InvaderSpeed = 3;
        private const int MothershipSpeed = 2;

        public InvaderType Type { get; }
        public int Score { get; private set; }

        public Invader(InvaderType type, Point location) : base(location, GetShipSize(type))
        {
            Type = type;
            SetScoreValue(type);
            if (Type == InvaderType.Mothership)
                MoveDistance = MothershipSpeed;
            else
                MoveDistance = InvaderSpeed;
        }

        public void Move(Point target)
        {
            int horizontalDistanceToTarget = Location.X - target.X;
            int verticalDistanceToTarget = Location.Y - target.Y;

            Direction direction = Direction.Undefined;
            int distance = MoveDistance;
            if (Math.Abs(horizontalDistanceToTarget) > Math.Abs(verticalDistanceToTarget))
            {
                if (horizontalDistanceToTarget < 0) direction = Direction.Right;
                else if (horizontalDistanceToTarget > 0) direction = Direction.Left;

                if (Math.Abs(horizontalDistanceToTarget) < MoveDistance) 
                    distance = horizontalDistanceToTarget;
            }
            else
            {
                if (verticalDistanceToTarget < 0) direction = Direction.Down;
                else if (verticalDistanceToTarget > 0) direction = Direction.Up;

                if (Math.Abs(verticalDistanceToTarget) < MoveDistance) 
                    distance = verticalDistanceToTarget;
            }
            
            if (direction != Direction.Undefined)
            {
                Move(direction, distance);
            }
        }

        private static Size GetShipSize(InvaderType type)
        {
            if (type == InvaderType.Mothership)
                return MothershipSize;
            else
                return InvaderSize;
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
                    throw new ArgumentException($"Unexpected invader type: {type}.");
            }
        }
    }
}