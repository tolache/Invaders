﻿using System.Drawing;

namespace Invaders.Model
{
    public abstract class Ship : AreaOccupier
    {
        public ShipStatus ShipStatus;

        protected Ship(Point location, Size size) : base(location, size)
        {
            ShipStatus = ShipStatus.AliveNormal;
        }

        public abstract void Move(Direction direction);
    }

    public enum ShipStatus
    {
        AliveNormal,
        AliveDiveBombing,
        Killed,
        OffScreen,
    }
}