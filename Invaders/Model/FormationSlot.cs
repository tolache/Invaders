using System;
using System.Drawing;

namespace Invaders.Model
{
    public class FormationSlot : IEquatable<FormationSlot>
    {
        private readonly IFormation _owner;
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

        public Point Location
        {
            get
            {
                int x = _owner.Location.X + (_owner.SlotSize.Width + _owner.Spacing) * Column;
                int y = _owner.Location.Y + (_owner.SlotSize.Height + _owner.Spacing) * Row;
                return new Point(x, y);
            }
        }

        public bool Occupied { get; set; }

        public FormationSlot(IFormation owner, int column, int row)
        {
            _owner = owner;
            Column = column;
            Row = row;
            Occupied = true;
        }

        public bool Equals(FormationSlot? other)
        {
            if (other == null)
            {
                return false;
            }
            if (Column == other.Column && Row == other.Row)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}