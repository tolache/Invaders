namespace Invaders.Model
{
    public class Vector
    {
        public Direction Direction { get; }
        public double Speed { get; }

        public Vector(Direction direction, double speed)
        {
            Direction = direction;
            Speed = speed;
        }
    }
}