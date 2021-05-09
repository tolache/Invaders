using System;
using System.Drawing;

namespace Invaders.Model
{
    public class Player : Ship
    {
        public const int Speed = 3;
        public static readonly Size PlayerSize = new(25,15);

        private const int BatterySize = 3;
        private const int BatteryChargeRateMs = 500;
        private DateTime _batteryChargeChange;
        private readonly TimeSpan _batteryChargeRate;

        public int CurrentBatteryCharge { get; private set; }
        public bool HasBatteryCharge => CurrentBatteryCharge > 0;

        public Player(Point location, Size size) : base(location, size)
        {
            CurrentBatteryCharge = BatterySize;
            _batteryChargeChange = DateTime.Now;
            _batteryChargeRate = TimeSpan.FromMilliseconds(BatteryChargeRateMs);
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

        public void ChargeBattery()
        {
            if (DateTime.Now - _batteryChargeChange < _batteryChargeRate) return;
            if (CurrentBatteryCharge < BatterySize)
            {
                CurrentBatteryCharge++;
            }
            _batteryChargeChange = DateTime.Now;
        }

        public void DrainBattery()
        {
            if (CurrentBatteryCharge <= 0) return;
            CurrentBatteryCharge--;
            _batteryChargeChange = DateTime.Now;
        }
    }
}