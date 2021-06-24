using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace Invaders.Model
{
    public class InvaderManager : ShipManager
    {
        private readonly Dictionary<Invader,FormationSlot?> _invaders = new();
        private readonly InvaderFormation _invaderFormation;
        private readonly Size _playAreaSize;
        private readonly Random _random = new();
        private Direction _mothershipDirection = Direction.Right;

        public bool MothershipCreationAttempted { get; set; }

        public InvaderManager(Size playAreaSize, OnShipChangedCallback onShipChanged) : base(onShipChanged)
        {
            _playAreaSize = playAreaSize;
            _invaderFormation = new InvaderFormation(_playAreaSize);
        }

        public ReadOnlyCollection<MovingBody> GetInvaders()
        {
            List<Invader> invaders = _invaders.Keys.ToList();
            return invaders.ConvertAll(_ => (MovingBody) _).AsReadOnly();
        }

        public void KillAllInvaders()
        {
            foreach (Invader invader in _invaders.Keys)
            {
                invader.ShipStatus = ShipStatus.Killed;
                
                OnShipChanged(invader);
            }

            _invaders.Clear();
            _invaderFormation.ResetFormation();
        }

        public void CreateInvaders(int wave)
        {
            for (int column = 0; column < 11; column++)
            {
                for (int row = 0; row < 6; row++)
                {
                    Point location = _invaderFormation.GetSlotLocation(column, row);
                    InvaderType type = GetInvaderType(wave, row);
                    Invader invader = new Invader(type, location, Invader.InvaderSize);
                    FormationSlot slot = new FormationSlot(_invaderFormation, column, row);
                    _invaders.Add(invader, slot);
                }
            }
        }

        public void MoveInvaders()
        {
            _invaderFormation.UpdateFormationLocation();
            
            foreach (Invader invader in _invaders.Keys.Where(_ => _.Type != InvaderType.Mothership))
            {
                Point target = _invaders[invader].Location;
                invader.Move(target);
            }

            if (_invaders.Keys.Any(invader => invader.Type == InvaderType.Mothership))
            {
                Invader mothership = _invaders.Keys.First(invader => invader.Type == InvaderType.Mothership);
                if (CheckMothershipReachedBorder())
                {
                    _invaders.Remove(mothership);
                    mothership.ShipStatus = ShipStatus.OffScreen;
                    OnShipChanged(mothership);
                }
                else
                {
                    mothership.Move(_mothershipDirection, mothership.MoveDistance);
                }
            }
        }

        public void UpdateAllInvaderShips()
        {
            foreach (Invader invader in _invaders.Keys)
            {
                invader.ShipStatus = ShipStatus.Alive;
                OnShipChanged(invader);
            }
        }

        public MovingBody DetermineShootingInvader()
        {
            var invaderColumns = from invader in _invaders.Keys
                group invader by invader.Location.X
                into invaderColumn
                select invaderColumn;
            var invaderColumnsList = invaderColumns.ToList();
            IGrouping<int, Invader> randomColumn =
                invaderColumnsList.ElementAt(_random.Next(invaderColumnsList.Count()));
            Invader shooter = randomColumn.ToList().Last();
            return shooter;
        }

        public void RemoveInvader(MovingBody invaderToRemove)
        {
            if (invaderToRemove is Invader invader)
            {
                _invaders.Remove(invader);
                invader.ShipStatus = ShipStatus.Killed;
                OnShipChanged(invader);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(invaderToRemove),
                    $"{nameof(MovingBody)} must be an {nameof(Invader)}.");
            }
        }

        public bool CheckInvadersReachedBottom(int invadersBottomBoundary)
        {
            // TODO: move this method to InvaderFormation
            var invadersReachedBottom = from invader in _invaders.Keys
                where invader.Location.Y >= invadersBottomBoundary - invader.Size.Height
                select invader;
            return invadersReachedBottom.Any();
        }

        public void TryCreateMothership()
        {
            if (!MothershipCreationAttempted && CheckGotSpaceForMothership() && CheckHaveHalfInvadersDied())
            {
                if (_random.Next(0, 3) == 2) CreateMothership();
                MothershipCreationAttempted = true;
            }
        }

        private static InvaderType GetInvaderType(int wave, int row)
        {
            return (wave, row) switch
            {
                (1, 5) => (InvaderType) 0,
                (1, 4) => (InvaderType) 0,
                (1, 3) => (InvaderType) 1,
                (1, 2) => (InvaderType) 1,
                (1, 1) => (InvaderType) 2,
                (1, 0) => (InvaderType) 3,
                
                (2, 5) => (InvaderType) 0,
                (2, 4) => (InvaderType) 1,
                (2, 3) => (InvaderType) 1,
                (2, 2) => (InvaderType) 1,
                (2, 1) => (InvaderType) 2,
                (2, 0) => (InvaderType) 3,
                
                (3, 5) => (InvaderType) 5,
                (3, 4) => (InvaderType) 1,
                (3, 3) => (InvaderType) 2,
                (3, 2) => (InvaderType) 2,
                (3, 1) => (InvaderType) 3,
                (3, 0) => (InvaderType) 3,
                
                (4, 5) => (InvaderType) 5,
                (4, 4) => (InvaderType) 5,
                (4, 3) => (InvaderType) 2,
                (4, 2) => (InvaderType) 3,
                (4, 1) => (InvaderType) 3,
                (4, 0) => (InvaderType) 4,
                _ => throw new ArgumentException($"Failed to determine invader type for wave '{wave}' and row '{row}'.")
            };
        }

        private int GetUppermostInvaderY()
        {
            // TODO: move this method to InvaderFormation
            int y = _playAreaSize.Height;
            foreach (Invader invader in _invaders.Keys)
            {
                if (invader.Location.Y < y)
                {
                    y = invader.Location.Y;
                }
            }
            return y;
        }

        private bool CheckGotSpaceForMothership()
        {
            int uppermostInvaderY = GetUppermostInvaderY();
            return uppermostInvaderY > Invader.MothershipSize.Height * 2;
        }

        private bool CheckHaveHalfInvadersDied()
        {
            return _invaders.Count <= 33;
        }

        private void CreateMothership()
        {
            Size size = new(Invader.MothershipSize.Width,Invader.MothershipSize.Height);
            int startY = (GetUppermostInvaderY() - Invader.MothershipSize.Height) / 2;
            int startX = 0;
            if (_random.Next(2) == 1)
            {
                startX = _playAreaSize.Width - Invader.MothershipSize.Width;
                _mothershipDirection = Direction.Left;
            }
            Point startLocation = new(startX, startY);
            Invader mothership = new Invader(InvaderType.Mothership, startLocation, size);
            _invaders.Add(mothership, null);
        }

        private bool CheckMothershipReachedBorder()
        {
            if (_invaders.Keys.Any(invader => invader.Type == InvaderType.Mothership))
            {
                Invader mothership = _invaders.Keys.First(invader => invader.Type == InvaderType.Mothership);
                if (_mothershipDirection == Direction.Right
                    && mothership.Location.X >
                    _playAreaSize.Width - mothership.Size.Width - Invader.InvaderSize.Width / 2
                    || _mothershipDirection == Direction.Left
                    && mothership.Location.X < Invader.InvaderSize.Width / 2)
                {
                    return true;
                }
            }

            return false;
        }
    }
}