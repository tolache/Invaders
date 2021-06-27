using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Invaders.Model
{
    public class InvaderFormation : Mover, IFormation
    {
        private const int FormationMoveIntervalMs = 1000;
        private readonly int _formationMoveDistance = Invader.InvaderSize.Width;
        private readonly FormationSlot[,] _formationSlots = new FormationSlot[11,6];
        private readonly Size _playAreaSize;
        private DateTime _nextFormationMoveTime = DateTime.MinValue;
        private Direction _nextMoveDirection;
        private bool _justMovedDown;

        public Size SlotSize => Invader.InvaderSize;
        public int Spacing => Convert.ToInt32(Invader.InvaderSize.Width * 0.5);

        public InvaderFormation(Size playAreaSize)
        {
            _playAreaSize = playAreaSize;
            _nextMoveDirection = Direction.Right;
            ResetFormation();
        }

        public void ResetFormation()
        {
            Location = new Point(Spacing, Spacing);
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    _formationSlots[i, j] = new FormationSlot(this, i, j);
                }
            }

            _justMovedDown = false;
            _nextMoveDirection = Direction.Right;
        }

        public void UpdateFormationLocation()
        {
            if (_nextFormationMoveTime > DateTime.Now) return;

            _justMovedDown = false;
            
            if (CheckFormationReachedRightBoundary())
            {
                Move(Direction.Down, _formationMoveDistance);
                _justMovedDown = true;
                _nextMoveDirection = Direction.Left;
            }
            
            if (CheckFormationReachedLeftBoundary())
            {
                Move(Direction.Down, _formationMoveDistance);
                _justMovedDown = true;
                _nextMoveDirection = Direction.Right;
            }
            
            if (!_justMovedDown)
            {
                Move(_nextMoveDirection, _formationMoveDistance);
            }

            _nextFormationMoveTime = DateTime.Now + TimeSpan.FromMilliseconds(FormationMoveIntervalMs);
        }

        public Point GetSlotLocation(int column, int row)
        {
            return _formationSlots[column, row].Location;
        }
        
        public void SetSlot(FormationSlot slot)
        {
            _formationSlots[slot.Column, slot.Row] = slot;
        }

        public bool CheckFormationReachedBottom(int formationBottomBoundary)
        {
            IEnumerable<FormationSlot> occupiedSlots = _formationSlots.Cast<FormationSlot>().Where(_ => _.Occupied).ToList();
            IEnumerable<FormationSlot> occupiedSlotsCloseToBottom = from slot in occupiedSlots
                where slot.Location.Y + SlotSize.Height >= formationBottomBoundary
                select slot;
            
            if (occupiedSlotsCloseToBottom.Any())
                return true;
            else
                return false;
        }
        
        public int GetUppermostInvaderY()
        {
            int y = _playAreaSize.Height;
            IEnumerable<FormationSlot> occupiedSlots = _formationSlots.Cast<FormationSlot>().Where(_ => _.Occupied).ToList();
            foreach (FormationSlot slot in occupiedSlots)
            {
                if (slot.Location.Y < y) 
                    y = slot.Location.Y;
            }
            return y;
        }

        private bool CheckFormationReachedRightBoundary()
        {
            var occupiedSlotsCloseRightBoundary = from FormationSlot slot in _formationSlots
                where slot.Occupied && slot.Location.X > _playAreaSize.Width - Invader.InvaderSize.Width * 2
                select slot;
            if (occupiedSlotsCloseRightBoundary.Any() && _nextMoveDirection == Direction.Right)
            {
                return true;
            }
            return false;
        }

        private bool CheckFormationReachedLeftBoundary()
        {
            var occupiedSlotsCloseLeftBoundary = from FormationSlot slot in _formationSlots
                where slot.Occupied && slot.Location.X < Invader.InvaderSize.Width
                select slot;
            if (occupiedSlotsCloseLeftBoundary.Any() && _nextMoveDirection == Direction.Left)
            {
                return true;
            }
            return false;
        }
    }

    public interface IFormation
    {
        public Point Location { get; }
        public Size SlotSize { get; }
        public int Spacing { get; }
    }
}