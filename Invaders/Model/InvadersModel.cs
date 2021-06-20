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
        private const int TotalWaves = 4;
        private readonly Size _playAreaSize = new(400, 300);
        private readonly PlayerManager _playerManager;
        private readonly InvaderManager _invaderManager;
        private readonly StarManager _starManager;
        private readonly ShotManager _shotManager;
        private readonly Random _random = new();

        public int Score { get; private set; }
        public int Wave { get; private set; }
        public int Lives { get; private set; }
        public bool GameOver { get; private set; }
        public bool Victory { get; private set; }
        
        private bool GamePaused { get; set; }


        public InvadersModel()
        {
            _playerManager = new PlayerManager(_playAreaSize, OnShipChanged);
            _invaderManager = new InvaderManager(_playAreaSize, OnShipChanged);
            _shotManager = new ShotManager(_playAreaSize, OnShotMoved);
            _starManager = new StarManager(_playAreaSize, OnStarChanged);
            EndGame();
        }
        
        public event EventHandler<ShipChangedEventArgs> ShipChanged;
        public event EventHandler<ShotMovedEventArgs> ShotMoved;
        public event EventHandler<StarChangedEventArgs> StarChanged;

        public void EndGame(bool victory = false)
        {
            GameOver = true;
            Victory = victory;
        }

        public void StartGame()
        {
            GameOver = false;
            Victory = false;
            _invaderManager.MothershipCreationAttempted = false;
            Score = 0;
            
            _invaderManager.KillAllInvaders();

            _shotManager.ClearAllShots();
            _starManager.RecreateStars();
            _playerManager.CreatePlayer();

            Lives = 2;
            Wave = 0;
            NextWave();
        }

        public void Update(bool paused)
        {
            _starManager.Twinkle();
            GamePaused = paused;
            if (GamePaused)
            {
                return;
            }

            if (Wave > TotalWaves)
            {
                EndGame(true);
                return;
            }
            
            if (_invaderManager.GetInvaders().Count == 0)
            {
                NextWave();
            }

            _playerManager.TryClearPlayerDiedStatus();
            _invaderManager.MoveInvaders();
            _invaderManager.TryCreateMothership();
            _shotManager.MoveShots();
            ReturnFire();
            DestroyHitInvaders();

            List<Shot> shotsHittingPlayer = GetShotsHittingPlayer();
            if (shotsHittingPlayer.Any())
            {
                _shotManager.RemoveShots(shotsHittingPlayer);
                HitPlayer();
            }
            UpdateAllShipsAndStars();

            int invaderBottomBoundary = _playerManager.GetPlayerStartLocation().Y;
            if (_invaderManager.CheckInvadersReachedBottom(invaderBottomBoundary))
            {
                EndGame();
            }
        }

        public void FireShot()
        {
            if (GameOver || GamePaused || !_playerManager.CheckCanPlayerShoot()) return;
            _playerManager.DrainPlayerBattery();
            _shotManager.AddShot(_playerManager.Player);
        }

        public void UpdateAllShipsAndStars()
        {
            _playerManager.UpdatePlayerShip();
            _invaderManager.UpdateAllInvaderShips();
            _starManager.UpdateAllStars();
            _shotManager.UpdateAllShots();
        }

        public void MovePlayer(Direction direction)
        {
            _playerManager.MovePlayer(direction);
        }

        private void NextWave()
        {
            Wave++;
            _invaderManager.MothershipCreationAttempted = false;
            if (Wave > TotalWaves)
            {
                return;
            }
            _invaderManager.KillAllInvaders();
            _invaderManager.CreateInvaders(Wave);
        }

        private bool RectsOverlap(Rectangle r1, Rectangle r2)
        {
            return r1.IntersectsWith(r2);
        }

        private void HitPlayer()
        {
            if (_playerManager.PlayerStatus != ShipStatus.Alive) return;
            _playerManager.DamagePlayer();
            Lives--;
            if (Lives < 0)
            {
                EndGame();
            }
        }

        private void DestroyHitInvaders()
        {
            List<Shot> playerShotsCopy = new List<Shot>(_shotManager.PlayerShots);
            List<MovingBody> invadersCopy = new List<MovingBody>(_invaderManager.GetInvaders());
            foreach (Shot shot in playerShotsCopy)
            {
                foreach (MovingBody areaOccupierInvader in invadersCopy.Where(invader => RectsOverlap(shot.Area, invader.Area)))
                {
                    _shotManager.RemoveShots(shot);
                    _invaderManager.RemoveInvader(areaOccupierInvader as Invader);
                    if (areaOccupierInvader is Invader invader) Score += invader.Score;
                }
            }
        }

        private void ReturnFire()
        {
            if (!CheckInvadersCanShoot())
            {
                return;
            }

            MovingBody shootingInvader = _invaderManager.DetermineShootingInvader();
            _shotManager.AddShot(shootingInvader);
        }
        
        private bool CheckInvadersCanShoot()
        {
            return _shotManager.InvaderShotsCount < Wave + 1 && _random.Next(30) >= 30 - Wave;
        }

        private List<Shot> GetShotsHittingPlayer()
        {
            List<Shot> result = new();
            List<Shot> invaderShotsCopy = new List<Shot>(_shotManager.InvaderShots);
            foreach (Shot shot in invaderShotsCopy.Where(shot => RectsOverlap(shot.Area, _playerManager.Player.Area)))
            {
                result.Add(shot);
            }
                
            return result;
        }

        private void OnShipChanged(Ship ship)
        {
            ShipChangedEventArgs e = new ShipChangedEventArgs(ship);
            ShipChanged?.Invoke(this, e);
        }

        private void OnShotMoved(Shot shot, bool disappeared)
        {
            ShotMovedEventArgs e = new ShotMovedEventArgs(shot, disappeared);
            ShotMoved?.Invoke(this, e);
        }

        private void OnStarChanged(Point location, bool disappeared)
        {
            StarChangedEventArgs e = new StarChangedEventArgs(location, disappeared);
            StarChanged?.Invoke(this, e);
        }
    }
}