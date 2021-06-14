using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace Invaders.Model
{
    public class InvaderManager : ShipManager
    {
        private const int MoveInvadersCooldownMs = 93;
        private readonly List<Invader> _invaders = new();
        private readonly int _horizontalInvaderSpacing = Convert.ToInt32(Invader.InvaderSize.Width * 0.5);
        private readonly int _verticalInvaderSpacing = Convert.ToInt32(Invader.InvaderSize.Height * 0.5);
        private readonly Size _playAreaSize;
        private readonly Random _random = new();
        private Direction _invaderDirection = Direction.Right;
        private Direction _mothershipDirection = Direction.Right;
        private DateTime _lastUpdated = DateTime.MinValue;
        private bool _justMovedDown;

        public bool MothershipCreationAttempted { get; set; }

        public InvaderManager(Size playAreaSize, OnShipChangedCallback onShipChanged) : base(onShipChanged)
        {
            _playAreaSize = playAreaSize;
        }
        
        public ReadOnlyCollection<MovingBody> GetInvaders()
        {
            return _invaders.ConvertAll(_ => (MovingBody) _).AsReadOnly();
        }

        public void KillAllInvaders()
        {
            foreach (Invader invader in _invaders)
            {
                invader.ShipStatus = ShipStatus.Killed;
                OnShipChanged(invader);
            }

            _invaders.Clear();
        }
        
        public void CreateInvaders(int wave)
        {
            {
                int invaderX = _horizontalInvaderSpacing;

                for (int invaderColumn = 1; invaderColumn <= 11; invaderColumn++)
                {
                    int invaderY = _verticalInvaderSpacing;

                    Invader[] currentColumn = new Invader[6];
                    for (int row = 6; row >= 1; row--)
                    {
                        Invader invader = new Invader(GetInvaderType(wave, row), new Point(invaderX, invaderY),
                            Invader.InvaderSize);
                        currentColumn[row - 1] = invader;

                        invaderY += Invader.InvaderSize.Height + _verticalInvaderSpacing;
                    }

                    _invaders.AddRange(currentColumn);

                    invaderX += Invader.InvaderSize.Width + _horizontalInvaderSpacing;
                }
            }
        }
        
        public void MoveInvaders()
        {
            TimeSpan updateInterval = TimeSpan.FromMilliseconds(MoveInvadersCooldownMs);
            if (DateTime.Now - _lastUpdated < updateInterval)
            {
                return;
            }
                
            _justMovedDown = false;

            IEnumerable<Invader> invadersCloseToRightBoundary = from invader in _invaders
                where invader.Location.X > _playAreaSize.Width - invader.Size.Width * 2 && invader.Type != InvaderType.Mothership
                select invader;
            if (invadersCloseToRightBoundary.Any() && _invaderDirection == Direction.Right)
            {
                foreach (Invader invader in _invaders.Where(invader => invader.Type != InvaderType.Mothership))
                {
                    invader.Move(Direction.Down);
                }
                _justMovedDown = true;
                _invaderDirection = Direction.Left;
            }
                
            IEnumerable<Invader> invadersCloseToLeftBoundary = from invader in _invaders
                where invader.Location.X < invader.Size.Width && invader.Type != InvaderType.Mothership
                select invader;
            if (invadersCloseToLeftBoundary.Any() && _invaderDirection == Direction.Left)
            {
                foreach (Invader invader in _invaders.Where(invader => invader.Type != InvaderType.Mothership))
                {
                    invader.Move(Direction.Down);
                }
                _justMovedDown = true;
                _invaderDirection = Direction.Right;
            }

            if (!_justMovedDown)
            {
                foreach (Invader invader in _invaders.Where(invader => invader.Type != InvaderType.Mothership))
                {
                    invader.Move(_invaderDirection);
                }
            }

            if (_invaders.Any(invader => invader.Type == InvaderType.Mothership))
            {
                Invader mothership = _invaders.First(invader => invader.Type == InvaderType.Mothership);
                if (CheckMothershipReachedBorder())
                {
                    _invaders.Remove(mothership);
                    mothership.ShipStatus = ShipStatus.OffScreen;
                    OnShipChanged(mothership);
                }
                else
                {
                    mothership.Move(_mothershipDirection);
                }
            }

            _lastUpdated = DateTime.Now;
        }
        
        public void UpdateAllInvaderShips()
        {
            foreach (Invader invader in _invaders)
            {
                invader.ShipStatus = ShipStatus.AliveNormal;
                OnShipChanged(invader);
            }
        }

        public MovingBody DetermineShootingInvader()
        {
            var invaderColumns = from invader in _invaders
                group invader by invader.Location.X
                into invaderColumn
                select invaderColumn;
            var invaderColumnsList = invaderColumns.ToList();
            IGrouping<int, Invader> randomColumn = invaderColumnsList.ElementAt(_random.Next(invaderColumnsList.Count()));
            Invader shooter = randomColumn.ToList().First();
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
            var invadersReachedBottom = from invader in _invaders
                where invader.Location.Y >= invadersBottomBoundary - _verticalInvaderSpacing
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
                (1, 1) => (InvaderType) 0,
                (1, 2) => (InvaderType) 0,
                (1, 3) => (InvaderType) 1,
                (1, 4) => (InvaderType) 1,
                (1, 5) => (InvaderType) 2,
                (1, 6) => (InvaderType) 3,
                (2, 1) => (InvaderType) 0,
                (2, 2) => (InvaderType) 1,
                (2, 3) => (InvaderType) 1,
                (2, 4) => (InvaderType) 1,
                (2, 5) => (InvaderType) 2,
                (2, 6) => (InvaderType) 3,
                (3, 1) => (InvaderType) 5,
                (3, 2) => (InvaderType) 1,
                (3, 3) => (InvaderType) 2,
                (3, 4) => (InvaderType) 2,
                (3, 5) => (InvaderType) 3,
                (3, 6) => (InvaderType) 3,
                (4, 1) => (InvaderType) 5,
                (4, 2) => (InvaderType) 5,
                (4, 3) => (InvaderType) 2,
                (4, 4) => (InvaderType) 3,
                (4, 5) => (InvaderType) 3,
                (4, 6) => (InvaderType) 4,
                _ => throw new ArgumentException($"Failed to determine invader type for wave '{wave}' and row '{row}'.")
            };
        }

        private int GetUppermostInvaderY()
        {
            int y = _playAreaSize.Height;
            foreach (Invader invader in _invaders)
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
            _invaders.Add(mothership);
        }

        private bool CheckMothershipReachedBorder()
        {
            if (_invaders.Any(invader => invader.Type == InvaderType.Mothership))
            {
                Invader mothership = _invaders.First(invader => invader.Type == InvaderType.Mothership);
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