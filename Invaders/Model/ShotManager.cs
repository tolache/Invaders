using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace Invaders.Model
{
    public class ShotManager
    {
        public ReadOnlyCollection<Shot> PlayerShots => _playerShots.AsReadOnly();
        public ReadOnlyCollection<Shot> InvaderShots => _invaderShots.AsReadOnly();
        public int InvaderShotsCount => _invaderShots.Count;
        
        private readonly List<Shot> _playerShots = new();
        private readonly List<Shot> _invaderShots = new();
        private readonly Size _playAreaSize;

        public ShotManager(Size playAreaSize, OnShotMovedCallback onShotMoved)
        {
            _onShotMoved = onShotMoved;
            _playAreaSize = playAreaSize;
        }

        private readonly OnShotMovedCallback _onShotMoved;

        public delegate void OnShotMovedCallback(Shot shot, bool disappeared);

        public void ClearAllShots()
        {
            foreach (Shot shot in _playerShots)
            {
                _onShotMoved(shot, true);
            }

            _playerShots.Clear();

            foreach (Shot shot in _invaderShots)
            {
                _onShotMoved(shot, true);
            }

            _invaderShots.Clear();
        }
        
        public void UpdateAllShots()
        {
            List<Shot> allShots = new List<Shot>();
            allShots.AddRange(_playerShots);
            allShots.AddRange(_invaderShots);
            foreach (Shot shot in allShots)
            {
                _onShotMoved(shot, false);
            }
        }

        public void AddShot(AreaOccupier shooter)
        {
            Point shotLocation = GetShotStartLocation(shooter);
            Direction shotDirection = shooter switch
            {
                Player => Direction.Up,
                Invader => Direction.Down,
                _ => throw new ArgumentOutOfRangeException(nameof(shooter), $"Unexpected shooter type: {shooter.GetType()}."),
            };
            Shot newShot = new(shotLocation, shotDirection);

            if (shooter is Player)
            {
                _playerShots.Add(newShot);
            }
            else
            {
                _invaderShots.Add(newShot);
            }
            
            _onShotMoved(newShot, false);
        }

        public void RemoveShots(Shot shot)
        {
            if (shot.Direction == Direction.Up)
            {
                _playerShots.Remove(shot);
            }
            else
            {
                _invaderShots.Remove(shot);
            }
            _onShotMoved(shot, true);
        }

        public void RemoveShots(IEnumerable<Shot> shots)
        {
            foreach (Shot shot in shots)
            {
                RemoveShots(shot);
            }
        }

        public void MoveShots()
        {
            foreach (Shot invaderShot in _invaderShots)
            {
                invaderShot.Move();
            }
    
            IEnumerable<Shot> visibleInvaderShots = from shot in _invaderShots
                where shot.Location.Y + shot.Size.Height < _playAreaSize.Height
                select shot;

            List<Shot> visibleInvaderShotsList = visibleInvaderShots.ToList();
            List<Shot> disappearedInvaderShots = _invaderShots.Except(visibleInvaderShotsList).ToList();
    
            _invaderShots.Clear();
            _invaderShots.AddRange(visibleInvaderShotsList);
    
            foreach (Shot shot in _invaderShots)
            {
                _onShotMoved(shot, false);
            }
    
            foreach (Shot shot in disappearedInvaderShots)
            {
                _onShotMoved(shot, true);
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
                _onShotMoved(shot, false);
            }
    
            foreach (Shot shot in disappearedPlayerShots)
            {
                _onShotMoved(shot, true);
            }
        }

        private Point GetShotStartLocation(AreaOccupier shooter)
        {
            int shotX = shooter.Location.X + shooter.Size.Width / 2;
            int shotY = shooter switch
            {
                Player => shooter.Location.Y - Shot.ShotSize.Height - 1,
                Invader => shooter.Location.Y + Shot.ShotSize.Height + 1,
                _ => throw new ArgumentOutOfRangeException(nameof(shooter), $"Unexpected shooter type: {shooter.GetType()}."),
            };
            return new Point(shotX, shotY);
        }
    }
}