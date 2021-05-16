using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Invaders.Model
{
    public sealed class InvadersModel
    {
        private static readonly Size PlayAreaSize = new(400, 300);
        private const int InitialStarCount = 50;
        private readonly TimeSpan _playerInvincibilityDuration = TimeSpan.FromMilliseconds(2500);
        private readonly TimeSpan _playerFreezeDuration = TimeSpan.FromMilliseconds(1500);

        private readonly Random _random = new();

        public int Score { get; private set; }
        public int Wave { get; private set; }
        public int Lives { get; private set; }

        public bool GameOver { get; private set; }
        public bool Victory { get; private set; }
        
        private bool GamePaused { get; set; }
        private DateTime? _playerDied;
        private ShipStatus PlayerStatus => _playerDied.HasValue ? ShipStatus.Killed : ShipStatus.AliveNormal;
        private bool PlayerFrozen => PlayerStatus == ShipStatus.Killed &&
                                     DateTime.Now - _playerDied < _playerFreezeDuration;

        private Player _player;

        private readonly List<Invader> _invaders = new();
        private readonly List<Shot> _playerShots = new();
        private readonly List<Shot> _invaderShots = new();
        private readonly List<Point> _stars = new();
        
        private Direction _invaderDirection = Direction.Right;
        private Direction _mothershipDirection = Direction.Right;
        private bool _justMovedDown;
        private DateTime _lastUpdated = DateTime.MinValue;

        private readonly int _horizontalInvaderSpacing = Convert.ToInt32(Invader.InvaderSize.Width * 0.5);
        private readonly int _verticalInvaderSpacing = Convert.ToInt32(Invader.InvaderSize.Height * 0.5);
        private bool _mothershipCreated;

        public InvadersModel()
        {
            EndGame();
        }

        public void EndGame(bool victory = false)
        {
            GameOver = true;
            Victory = victory;
        }

        public void StartGame()
        {
            GameOver = false;
            Victory = false;
            _mothershipCreated = false;
            Score = 0;
            foreach (Invader invader in _invaders)
            {
                invader.ShipStatus = ShipStatus.Killed;
                OnShipChanged(invader);
            }
            _invaders.Clear();
            
            foreach (Shot shot in _playerShots)
            {
                OnShotMoved(shot, true);
            }
            _playerShots.Clear();
            
            foreach (Shot shot in _invaderShots)
            {
                OnShotMoved(shot, true);
            }
            _invaderShots.Clear();
            
            foreach (var star in _stars)
            {
                OnStarChanged(star, true);
            }
            _stars.Clear();

            for (int i = 0; i < InitialStarCount; i++)
            {
                CreateStar();
            }
            
            _player = new Player(GetPlayerStartLocation(), Player.PlayerSize);
            _player.ShipStatus = ShipStatus.AliveNormal;
            OnShipChanged(_player);
            Lives = 2;
            Wave = 0;
            NextWave();

            Point GetPlayerStartLocation()
            {
                int playerStartingLocationX = (PlayAreaSize.Width + Player.PlayerSize.Width) / 2;
                int playerStartingLocationY = PlayAreaSize.Height - Convert.ToInt32(Player.PlayerSize.Height * 1.3);
                return new Point(playerStartingLocationX, playerStartingLocationY);
            }
        }

        public void FireShot()
        {
            if (GameOver || GamePaused || PlayerFrozen || !_player.HasBatteryCharge) return;
            
            int shotX = _player.Location.X + _player.Size.Width / 2;
            int shotY = _player.Location.Y - Shot.ShotSize.Height - 1;
            Point shotLocation = new(shotX, shotY);
            Shot shot = new Shot(shotLocation, Direction.Up);
            _playerShots.Add(shot);
            OnShotMoved(shot, false);
            
            _player.DrainBattery();
        }

        public void MovePlayer(Direction direction)
        {
            if (PlayerFrozen) return;
            if (PlayerReachedBoundary()) return;

            _player.Move(direction);
            _player.ShipStatus = PlayerStatus;
            OnShipChanged(_player);

            bool PlayerReachedBoundary()
            {
                switch (direction)
                {
                    case Direction.Left when _player.Location.X < Player.Speed:
                    case Direction.Right when _player.Location.X > PlayAreaSize.Width - _player.Size.Width - Player.Speed:
                        return true;
                    default:
                        return false;
                }
            }
        }

        private void CreateStar()
        {
            int s = _random.Next(5, 20);
            Size size = new Size(s, s);
            int x = _random.Next(0, PlayAreaSize.Width - size.Width);
            int y = _random.Next(0, PlayAreaSize.Height - size.Height);
            Point location = new Point(x, y);
            _stars.Add(location);
            OnStarChanged(location, false);
        }

        public void Update(bool paused)
        {
            Twinkle();
            GamePaused = paused;
            if (GamePaused)
            {
                return;
            }

            if (Wave == 4)
            {
                EndGame(true);
                return;
            }
            if (_invaders.Count == 0)
            {
                NextWave();
            }

            if (DateTime.Now - _playerDied > _playerInvincibilityDuration)
            {
                _playerDied = null;
            }
            
            if (Wave == 3 && !_mothershipCreated && CheckCanCreateMothership())
            {
                CreateMothership();
            }

            MoveInvaders();
            MoveShots();
            ReturnFire();
            DestroyHitInvaders();
            if (PlayerStatus == ShipStatus.AliveNormal)
            {
                DestroyHitPlayer();
            }
            UpdateAllShipsAndStars();
            CheckInvadersReachedBottom();
            
            void Twinkle()
            {
                if (_stars.Count < InitialStarCount * 0.85 ||
                    _stars.Count >= InitialStarCount * 0.85 && _stars.Count < InitialStarCount * 1.5 &&
                    _random.Next(0, 2) == 1)
                {
                    CreateStar();
                }
                else
                {
                    RemoveStar();
                }
            
                return;

                void RemoveStar()
                {
                    int index = _random.Next(0, _stars.Count);
                    Point star = _stars[index];
                    _stars.RemoveAt(index);
                    OnStarChanged(star, true);
                }
            }
            
            void MoveInvaders()
            {
                TimeSpan updateInterval = TimeSpan.FromMilliseconds(500);
                if (DateTime.Now - _lastUpdated < updateInterval)
                {
                    return;
                }
                
                _justMovedDown = false;

                IEnumerable<Invader> invadersCloseToRightBoundary = from invader in _invaders
                    where invader.Location.X > PlayAreaSize.Width - invader.Size.Width * 2 && invader.Type != InvaderType.Mothership
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

            void MoveShots()
            {
                foreach (Shot invaderShot in _invaderShots)
                {
                    invaderShot.Move();
                }
    
                IEnumerable<Shot> visibleInvaderShots = from shot in _invaderShots
                    where shot.Location.Y + Shot.ShotSize.Height < PlayAreaSize.Height
                    select shot;

                List<Shot> visibleInvaderShotsList = visibleInvaderShots.ToList();
                List<Shot> disappearedInvaderShots = _invaderShots.Except(visibleInvaderShotsList).ToList();
    
                _invaderShots.Clear();
                _invaderShots.AddRange(visibleInvaderShotsList);
    
                foreach (Shot shot in _invaderShots)
                {
                    OnShotMoved(shot, false);
                }
    
                foreach (Shot shot in disappearedInvaderShots)
                {
                    OnShotMoved(shot, true);
                }
    
                foreach (Shot shot in _playerShots)
                {
                    shot.Move();
                }

                IEnumerable<Shot> visiblePlayerShots = from shot in _playerShots
                    where shot.Location.Y > 0 
                    select shot;

                List<Shot> visiblePlayerShotsList = visiblePlayerShots.ToList();
                List<Shot> disappearedPlayerShots = _playerShots.Except(visiblePlayerShotsList).ToList();
    
                _playerShots.Clear();
                _playerShots.AddRange(visiblePlayerShotsList);
    
                foreach (Shot shot in _playerShots)
                {
                    OnShotMoved(shot, false);
                }
    
                foreach (Shot shot in disappearedPlayerShots)
                {
                    OnShotMoved(shot, true);
                }
            }

            void ReturnFire()
            {
                if (!InvadersCanShoot())
                {
                    return;
                }

                Invader shootingInvader = DetermineShootingInvader();
                Point shotLocation = GetShotStartLocation(shootingInvader);
                Shot newShot = new Shot(shotLocation, Direction.Down);
                _invaderShots.Add(newShot);

                Invader DetermineShootingInvader()
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

                bool InvadersCanShoot()
                {
                    if (_invaderShots.Count >= Wave + 1 || _random.Next(30) < 30 - Wave)
                    {
                        return false;
                    }

                    return true;
                }
                
                Point GetShotStartLocation(Invader shooter)
                {
                    int shotX = shooter.Location.X + shooter.Size.Width / 2;
                    int shotY = shooter.Location.Y + shooter.Size.Width + 1;
                    return new Point(shotX, shotY);
                }
            }

            void DestroyHitInvaders()
            {
                List<Shot> playerShotsCopy = new List<Shot>(_playerShots);
                List<Invader> invadersCopy = new List<Invader>(_invaders);
                foreach (Shot shot in playerShotsCopy)
                {
                    foreach (Invader invader in invadersCopy.Where(invader => RectsOverlap(shot.Area, invader.Area)))
                    {
                        _playerShots.Remove(shot);
                        OnShotMoved(shot, true);
                        
                        _invaders.Remove(invader);
                        invader.ShipStatus = ShipStatus.Killed;
                        OnShipChanged(invader);
                        Score += invader.Score;
                    }
                }
            }

            void DestroyHitPlayer()
            {
                List<Shot> invaderShotsCopy = new List<Shot>(_invaderShots);
                foreach (Shot shot in invaderShotsCopy.Where(shot => RectsOverlap(shot.Area, _player.Area)))
                {
                    _invaderShots.Remove(shot);
                    OnShotMoved(shot, true);
                    _playerDied = DateTime.Now;
                    Lives--;
                    _player.ShipStatus = PlayerStatus;
                    OnShipChanged(_player);
                    if (Lives < 0)
                    {
                        EndGame();
                    }
                }
            }

            void CheckInvadersReachedBottom()
            {
                var invadersReachedBottom = from invader in _invaders
                    where invader.Location.Y >= _player.Location.Y - _verticalInvaderSpacing
                    select invader;
                if (invadersReachedBottom.Any())
                {
                    EndGame();
                }
            }

            bool RectsOverlap(Rectangle r1, Rectangle r2)
            {
                if (r1.IntersectsWith(r2))
                {
                    return true;
                }

                return false;
            }
        }

        public void UpdateAllShipsAndStars()
        {
            if (_player != null)
            {
                _player.ChargeBattery();
                _player.ShipStatus = PlayerStatus;
                OnShipChanged(_player);
            }

            foreach (Invader invader in _invaders)
            {
                invader.ShipStatus = ShipStatus.AliveNormal;
                OnShipChanged(invader);
            }

            foreach (Point star in _stars)
            {
                OnStarChanged(star, false);
            }
            
            List<Shot> allShots = new List<Shot>();
            allShots.AddRange(_playerShots);
            allShots.AddRange(_invaderShots);
            foreach (Shot shot in allShots)
            {
                OnShotMoved(shot, false);
            }
        }

        private bool CheckMothershipReachedBorder()
        {
            if (_invaders.Any(invader => invader.Type == InvaderType.Mothership))
            {
                Invader mothership = _invaders.First(invader => invader.Type == InvaderType.Mothership);
                if (_mothershipDirection == Direction.Right
                    && mothership.Location.X >
                    PlayAreaSize.Width - mothership.Size.Width - Invader.InvaderSize.Width / 2
                    || _mothershipDirection == Direction.Left
                    && mothership.Location.X < Invader.InvaderSize.Width / 2)
                {
                    return true;
                }
            }

            return false;
        }

        private void NextWave()
        {
            Wave++;
            if (Wave == 4)
            {
                return;
            }
            _invaders.Clear();

            int invaderX = _horizontalInvaderSpacing;

            for (int invaderColumn = 1; invaderColumn <= 11; invaderColumn++)
            {
                int invaderY = _verticalInvaderSpacing;
                
                Invader[] currentColumn = new Invader[6];
                for (int row = 6; row >= 1; row--)
                {
                    Invader invader = new Invader(GetInvaderType(Wave, row), new Point(invaderX, invaderY), Invader.InvaderSize);
                    currentColumn[row - 1] = invader;
                    
                    invaderY += Invader.InvaderSize.Height + _verticalInvaderSpacing;
                }
                _invaders.AddRange(currentColumn);
                
                invaderX += Invader.InvaderSize.Width + _horizontalInvaderSpacing;
            }

            return;

            InvaderType GetInvaderType(int wave, int row)
            {
                switch (wave, row)
                {
                    case(1, 1):
                        return (InvaderType) 0;
                    case (1, 2):
                        return (InvaderType) 0;
                    case (1, 3):
                        return (InvaderType) 1;
                    case (1, 4):
                        return (InvaderType) 1;
                    case (1, 5):
                        return (InvaderType) 2;
                    case (1, 6):
                        return (InvaderType) 3;
                
                    case(2, 1):
                        return (InvaderType) 0;
                    case (2, 2):
                        return (InvaderType) 1;
                    case (2, 3):
                        return (InvaderType) 1;
                    case (2, 4):
                        return (InvaderType) 1;
                    case (2, 5):
                        return (InvaderType) 2;
                    case (2, 6):
                        return (InvaderType) 3;
                
                    case(3, 1):
                        return (InvaderType) 0;
                    case (3, 2):
                        return (InvaderType) 1;
                    case (3, 3):
                        return (InvaderType) 1;
                    case (3, 4):
                        return (InvaderType) 2;
                    case (3, 5):
                        return (InvaderType) 2;
                    case (3, 6):
                        return (InvaderType) 3;
                    default:
                        throw new NotImplementedException($"Failed to determine invader type for wave {wave} and row {row}.");
                }
            }
        }

        private bool CheckCanCreateMothership()
        {
            bool haveSpace = false;
            bool halfInvadersDied = false;
            int uppermostInvaderY = GetUppermostInvaderY();
            if (uppermostInvaderY > Invader.MothershipSize.Height * 2)
                haveSpace = true;
            if (_invaders.Count <= 33)
                halfInvadersDied = true;
            
            if (haveSpace && halfInvadersDied)
            {
                return true;
            }
        
            return false;
        }
        
        private int GetUppermostInvaderY()
        {
            int y = PlayAreaSize.Height;
            foreach (Invader invader in _invaders)
            {
                if (invader.Location.Y < y)
                {
                    y = invader.Location.Y;
                }
            }
            return y;
        }
        
        private void CreateMothership()
        {
            Size size = new(Invader.MothershipSize.Width,Invader.MothershipSize.Height);
            int startY = (GetUppermostInvaderY() - Invader.MothershipSize.Height) / 2;
            int startX = 0;
            if (_random.Next(2) == 1)
            {
                startX = PlayAreaSize.Width - Invader.MothershipSize.Width;
                _mothershipDirection = Direction.Left;
            }
            Point startLocation = new(startX, startY);
            Invader mothership = new Invader(InvaderType.Mothership, startLocation, size);
            _invaders.Add(mothership);
            _mothershipCreated = true;
        }

        public event EventHandler<ShipChangedEventArgs> ShipChanged;

        private void OnShipChanged(Ship ship)
        {
            ShipChangedEventArgs e = new ShipChangedEventArgs(ship);
            ShipChanged?.Invoke(this, e);
        }

        public event EventHandler<ShotMovedEventArgs> ShotMoved;

        private void OnShotMoved(Shot shot, bool disappeared)
        {
            ShotMovedEventArgs e = new ShotMovedEventArgs(shot, disappeared);
            ShotMoved?.Invoke(this, e);
        }

        public event EventHandler<StarChangedEventArgs> StarChanged;

        private void OnStarChanged(Point location, bool disappeared)
        {
            StarChangedEventArgs e = new StarChangedEventArgs(location, disappeared);
            StarChanged?.Invoke(this, e);
        }
    }
}