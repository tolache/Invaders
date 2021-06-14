using System;
using System.Drawing;

namespace Invaders.Model
{
    public class PlayerManager : ShipManager
    {
        public AreaOccupier Player => _player;
        public ShipStatus PlayerStatus => _playerDied.HasValue ? ShipStatus.Killed : ShipStatus.AliveNormal;

        private readonly TimeSpan _playerInvincibilityDuration = TimeSpan.FromMilliseconds(2500);
        private readonly TimeSpan _playerFreezeDuration = TimeSpan.FromMilliseconds(1500);
        private readonly Size _playAreaSize;
        private Player _player;
        private DateTime? _playerDied;

        public PlayerManager(Size playAreaSize, OnShipChangedCallback onShipChanged) : base(onShipChanged)
        {
            _playAreaSize = playAreaSize;
        }
        
        public void CreatePlayer()
        {
            _player = new Player(GetPlayerStartLocation(), Model.Player.PlayerSize) {ShipStatus = ShipStatus.AliveNormal};
            OnShipChanged(_player);
        }

        public void UpdatePlayerShip()
        {
            if (_player != null)
            {
                _player.ChargeBattery();
                _player.ShipStatus = PlayerStatus;
                OnShipChanged(_player);
            }
        }
        
        public void DamagePlayer()
        {
            _playerDied = DateTime.Now;
            _player.ShipStatus = PlayerStatus;
            OnShipChanged(_player);
        }
        
        public void TryClearPlayerDiedStatus()
        {
            if (DateTime.Now - _playerDied > _playerInvincibilityDuration)
            {
                _playerDied = null;
            }
        }
        
        public Point GetPlayerStartLocation()
        {
            int playerStartingLocationX = (_playAreaSize.Width + Model.Player.PlayerSize.Width) / 2;
            int playerStartingLocationY = _playAreaSize.Height - Convert.ToInt32(Model.Player.PlayerSize.Height * 1.3);
            return new Point(playerStartingLocationX, playerStartingLocationY);
        }

        public void MovePlayer(Direction direction)
        {
            if (CheckIsPlayerFrozen() || CheckPlayerReachedBoundary(direction)) return;

            _player.Move(direction);
            _player.ShipStatus = PlayerStatus;
            OnShipChanged(_player);
        }

        public bool CheckCanPlayerShoot()
        {
            return !CheckIsPlayerFrozen() && _player.HasBatteryCharge;
        }

        private bool CheckIsPlayerFrozen()
        {
            return PlayerStatus == ShipStatus.Killed &&
                   DateTime.Now - _playerDied < _playerFreezeDuration;
        }

        public void DrainPlayerBattery()
        {
            _player.DrainBattery();
        }

        private bool CheckPlayerReachedBoundary(Direction direction)
        {
            bool isPlayerNextToLeftBoundary = Player.Location.X < _player.Speed;
            bool isPlayerNextToRightBoundary = Player.Location.X > _playAreaSize.Width - Player.Size.Width - _player.Speed;
            if (direction == Direction.Left && isPlayerNextToLeftBoundary ||
                direction == Direction.Right && isPlayerNextToRightBoundary)
                return true;
            else
                return false;
        }
    }
}