namespace Invaders.Model
{
    public class Vector
    {
        public Direction Direction { get; set; }
        public double Speed { get; set; }

        public Vector(Direction direction, double speed)
        {
            Direction = direction;
            Speed = speed;
        }
    }
}