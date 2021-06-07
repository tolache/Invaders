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
        private Player _player;
        private DateTime? _playerDied;

        public PlayerManager(OnShipChangedDelegate onShipChanged) : base(onShipChanged)
        {
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
            int playerStartingLocationX = (InvadersModel.PlayAreaSize.Width + Model.Player.PlayerSize.Width) / 2;
            int playerStartingLocationY = InvadersModel.PlayAreaSize.Height - Convert.ToInt32(Model.Player.PlayerSize.Height * 1.3);
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
            bool isPlayerNextToLeftBoundary = Player.Location.X < Model.Player.Speed;
            bool isPlayerNextToRightBoundary = Player.Location.X > InvadersModel.PlayAreaSize.Width - Player.Size.Width - Model.Player.Speed;
            if (direction == Direction.Left && isPlayerNextToLeftBoundary ||
                direction == Direction.Right && isPlayerNextToRightBoundary)
                return true;
            else
                return false;
        }
    }
}