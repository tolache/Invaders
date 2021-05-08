using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Invaders.Model
{
    public class InvadersModel
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
        public bool GamePaused { get; private set; }
        
        private DateTime? _playerDied;
        private bool PlayerDying => _playerDied.HasValue;
        private bool PlayerFrozen => PlayerDying && DateTime.Now - _playerDied < _playerFreezeDuration;

        private Player _player;

        private readonly List<Invader> _invaders = new();
        private readonly List<Shot> _playerShots = new();
        private readonly List<Shot> _invaderShots = new();
        private readonly List<Point> _stars = new();
        
        private Direction _invaderDirection = Direction.Right;
        private bool _justMovedDown = false;
        private DateTime _lastUpdated = DateTime.MinValue;

        private readonly int _horizontalInvaderSpacing = Convert.ToInt32(Invader.InvaderSize.Width * 0.5);
        private readonly int _verticalInvaderSpacing = Convert.ToInt32(Invader.InvaderSize.Height * 0.5);

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
            Score = 0;
            foreach (Invader invader in _invaders)
            {
                OnShipChanged(invader, true);
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
            OnShipChanged(_player, false);
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
            OnShipChanged(_player, PlayerDying);

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
            
            MoveInvaders();
            MoveShots();
            ReturnFire();
            CheckForInvaderCollisions();
            if (!PlayerDying)
            {
                CheckForPlayerCollisions();
            }
            UpdateAllShipsAndStars();
            CheckIfInvadersReachedBottom();
            
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
                TimeSpan updateInterval = TimeSpan.FromMilliseconds(1000);
                if (DateTime.Now - _lastUpdated < updateInterval)
                {
                    // TODO: check and update the private framesSkipped field??
                    return;
                }
                
                _justMovedDown = false;

                IEnumerable<Invader> invadersCloseToRightBoundary = from invader in _invaders
                    where invader.Location.X > PlayAreaSize.Width - invader.Size.Width * 3
                    select invader;
                if (invadersCloseToRightBoundary.Any() && _invaderDirection == Direction.Right)
                {
                    foreach (Invader invader in _invaders)
                    {
                        invader.Move(Direction.Down);
                    }
                    _justMovedDown = true;
                    _invaderDirection = Direction.Left;
                }
                
                IEnumerable<Invader> invadersCloseToLeftBoundary = from invader in _invaders
                    where invader.Location.X < invader.Size.Width * 2
                    select invader;
                if (invadersCloseToLeftBoundary.Any() && _invaderDirection == Direction.Left)
                {
                    foreach (Invader invader in _invaders)
                    {
                        invader.Move(Direction.Down);
                    }
                    _justMovedDown = true;
                    _invaderDirection = Direction.Right;
                }

                if (!_justMovedDown)
                {
                    foreach (Invader invader in _invaders)
                    {
                        invader.Move(_invaderDirection);
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

            void CheckForInvaderCollisions()
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
                        OnShipChanged(invader, true);
                        Score += invader.Score;
                    }
                }
            }

            void CheckForPlayerCollisions()
            {
                List<Shot> invaderShotsCopy = new List<Shot>(_invaderShots);
                foreach (Shot shot in invaderShotsCopy.Where(shot => RectsOverlap(shot.Area, _player.Area)))
                {
                    _invaderShots.Remove(shot);
                    OnShotMoved(shot, true);
                    _playerDied = DateTime.Now;
                    Lives--;
                    OnShipChanged(_player, PlayerDying);
                }
            }

            void CheckIfInvadersReachedBottom()
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
            _player?.ChargeBattery();

            OnShipChanged(_player, PlayerDying);
            
            foreach (Invader invader in _invaders)
            {
                OnShipChanged(invader, false);
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

        public event EventHandler<ShipChangedEventArgs> ShipChanged;
        protected virtual void OnShipChanged(Ship ship, bool killed)
        {
            ShipChangedEventArgs e = new ShipChangedEventArgs(ship, killed);
            ShipChanged?.Invoke(this, e);
        }

        public event EventHandler<ShotMovedEventArgs> ShotMoved;
        protected virtual void OnShotMoved(Shot shot, bool disappeared)
        {
            ShotMovedEventArgs e = new ShotMovedEventArgs(shot, disappeared);
            ShotMoved?.Invoke(this, e);
        }

        public event EventHandler<StarChangedEventArgs> StarChanged;
        protected virtual void OnStarChanged(Point location, bool disappeared)
        {
            StarChangedEventArgs e = new StarChangedEventArgs(location, disappeared);
            StarChanged?.Invoke(this, e);
        }
    }
}