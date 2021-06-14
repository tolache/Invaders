using System;
using System.Drawing;

namespace Invaders.Model
{
    public class Player : Ship
    {
        public static readonly Size PlayerSize = new(25,15);

        private const int PlayerSpeed = 3;
        private const int BatterySize = 3;
        private const int BatteryChargeTimeMs = 400;
        private DateTime _batteryChargeChange;
        private readonly TimeSpan _batteryChargeRate;

        public int CurrentBatteryCharge { get; private set; }
        public bool HasBatteryCharge => CurrentBatteryCharge > 0;

        public Player(Point location, Size size) : base(location, size)
        {
            Speed = PlayerSpeed;
            CurrentBatteryCharge = BatterySize;
            _batteryChargeChange = DateTime.Now;
            _batteryChargeRate = TimeSpan.FromMilliseconds(BatteryChargeTimeMs);
        }

        public override void Move(Direction direction)
        {
            Point oldLocation = Location;
            Location = direction switch
            {
                Direction.Left => new Point(oldLocation.X - Speed, oldLocation.Y),
                Direction.Right => new Point(oldLocation.X + Speed, oldLocation.Y),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), $"Invalid direction '{direction}' for '{GetType()}'."),
            };
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