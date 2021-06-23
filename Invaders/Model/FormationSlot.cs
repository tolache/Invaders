using System;
using System.Drawing;

namespace Invaders.Model
{
    public class FormationSlot
    {
        private readonly IFormation _owner;
        private readonly FormationPosition _position;

        public Point Location
        {
            get
            {
                int x = _owner.Location.X + (_owner.SlotSize.Width + _owner.Spacing) * _position.Column;
                int y = _owner.Location.Y + (_owner.SlotSize.Height + _owner.Spacing) * _position.Row;
                return new Point(x, y);
            }
        }

        public bool Occupied { get; private set; }

        public FormationSlot(IFormation owner, int column, int row)
        {
            _owner = owner;
            _position = new FormationPosition(column, row);
            Occupied = true;
        }
    }

    public class FormationPosition
    {
        private readonly int _column;
        private readonly int _row;
        public int Column
        {
            get => _column;
            init 
            {
                if (value is >= 0 and <= 10)
                {
                    _column = value;
                }
                else throw new ArgumentOutOfRangeException(nameof(Column), "Must be between 0 and 10");
            }
        }

        public int Row
        { 
            get => _row;
            init 
            {
                if (value is >= 0 and <= 5)
                {
                    _row = value;
                }
                else throw new ArgumentOutOfRangeException(nameof(Row), "Must be between 0 and 5");
            }
            
        }

        public FormationPosition(int column, int row)
        {
            Column = column;
            Row = row;
        }
    }
}