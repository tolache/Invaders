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
        private readonly PlayerManager _playerManager;
        private readonly InvaderManager _invaderManager;
        private readonly StarManager _starManager;
        private readonly ShotManager _shotManager;
        private readonly Random _random = new();
        private bool _mothershipCreationAttempted;
        
        public static readonly Size PlayAreaSize = new(400, 300);
        public int Score { get; private set; }
        public int Wave { get; private set; }
        public int Lives { get; private set; }
        public bool GameOver { get; private set; }
        public bool Victory { get; private set; }
        
        private bool GamePaused { get; set; }


        public InvadersModel()
        {
            _playerManager = new PlayerManager(OnShipChanged);
            _invaderManager = new InvaderManager(OnShipChanged);
            _starManager = new StarManager(OnStarChanged);
            _shotManager = new ShotManager(OnShotMoved);
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
            _mothershipCreationAttempted = false;
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
            if (_invaderManager.Invaders.Count == 0)
            {
                NextWave();
            }

            _playerManager.TryClearPlayerDiedStatus();
            
            if (!_mothershipCreationAttempted && _invaderManager.CheckCanCreateMothership())
            {
                if (_random.Next(0,3) == 2) _invaderManager.CreateMothership();
                _mothershipCreationAttempted = true;
            }

            _invaderManager.MoveInvaders();
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
            if (_invaderManager.CheckInvadersReachedBottom(_playerManager.GetPlayerStartLocation().Y))
            {
                EndGame();
            }

            void ReturnFire()
            {
                if (!InvadersCanShoot())
                {
                    return;
                }

                AreaOccupier shootingInvader = _invaderManager.DetermineShootingInvader();
                _shotManager.AddShot(shootingInvader);
                
                bool InvadersCanShoot()
                {
                    if (_shotManager.InvaderShotsCount >= Wave + 1 || _random.Next(30) < 30 - Wave)
                    {
                        return false;
                    }
                        
                    return true;
                }
            }

            void DestroyHitInvaders()
            {
                List<Shot> playerShotsCopy = new List<Shot>(_shotManager.PlayerShots);
                List<AreaOccupier> invadersCopy = new List<AreaOccupier>(_invaderManager.Invaders);
                foreach (Shot shot in playerShotsCopy)
                {
                    foreach (AreaOccupier areaOccupierInvader in invadersCopy.Where(invader => RectsOverlap(shot.Area, invader.Area)))
                    {
                        _shotManager.RemoveShots(shot);
                        _invaderManager.RemoveInvader(areaOccupierInvader as Invader);
                        if (areaOccupierInvader is Invader invader) Score += invader.Score;
                    }
                }
            }
            
            List<Shot> GetShotsHittingPlayer()
            {
                List<Shot> result = new();
                List<Shot> invaderShotsCopy = new List<Shot>(_shotManager.InvaderShots);
                foreach (Shot shot in invaderShotsCopy.Where(shot => RectsOverlap(shot.Area, _playerManager.Player.Area)))
                {
                    result.Add(shot);
                }
                
                return result;
            }
        }
        
        private bool RectsOverlap(Rectangle r1, Rectangle r2)
        {
            return r1.IntersectsWith(r2);
        }

        private void HitPlayer()
        {
            if (_playerManager.PlayerStatus != ShipStatus.AliveNormal) return;
            _playerManager.DamagePlayer();
            Lives--;
            if (Lives < 0)
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
            _mothershipCreationAttempted = false;
            if (Wave > TotalWaves)
            {
                return;
            }
            _invaderManager.KillAllInvaders();
            _invaderManager.CreateInvaders(Wave);
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