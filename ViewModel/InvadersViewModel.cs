using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Invaders.Model;
using Invaders.View;

namespace Invaders.ViewModel
{
    public class InvadersViewModel : INotifyPropertyChanged
    {
        public INotifyCollectionChanged Sprites => _sprites;
        public ObservableCollection<object> Lives => _lives;
        public bool GameOver => _model.GameOver;
        public bool Victory => _model.Victory;
        public bool Paused { get; set; }
        public int Score { get; private set; }
        public int Wave { get; private set; }

        private static double Scale { get; set; }
        private readonly AudioPlaybackViewModel _audioPlaybackViewModel;
        private readonly ObservableCollection<FrameworkElement> _sprites = new();
        private readonly ObservableCollection<object> _lives = new();
        private bool _lastPaused = true;
        private int _lastPlayerBatteryCharge;

        public Size PlayAreaSize
        {
            set
            {
                Scale = value.Width / 405;
                _model.UpdateAllShipsAndStars();
                RecreateScanLines();
            }
        }

        private void RecreateScanLines()
        {
            foreach (FrameworkElement scanLine in _scanLines)
            {
                _sprites.Remove(scanLine);
            }
            _scanLines.Clear();

            
            for (int y = 2; y < 300; y += 4)
            {
                FrameworkElement scanLineToAdd = InvadersHelper.ScanLineFactory(y, Scale);
                _scanLines.Add(scanLineToAdd);
                _sprites.Add(scanLineToAdd);
            }
        }

        private readonly InvadersModel _model = new();
        private readonly DispatcherTimer _timer = new();
        private FrameworkElement _playerControl = null;
        private bool _playerHitAnimationInProgress = false;
        private readonly Dictionary<Invader, FrameworkElement> _invaders = new();

        private readonly Dictionary<FrameworkElement, DateTime> _shotInvaders = new();

        private readonly Dictionary<Shot, FrameworkElement> _shots = new();
        private readonly Dictionary<System.Drawing.Point, FrameworkElement> _stars = new();
        private readonly Dictionary<FrameworkElement, DateTime> _fadedStars = new();
        private readonly List<FrameworkElement> _scanLines = new();
        private DateTime? _leftAction = null;
        private DateTime? _rightAction = null;
        
        public InvadersViewModel()
        {
            Scale = 1;
            
            _audioPlaybackViewModel = new AudioPlaybackViewModel();

            _model.ShipChanged += OnModelShipChanged;
            _model.ShotMoved += OnModelShotMoved;
            _model.StarChanged += OnModelStarChanged;

            _timer.Interval = TimeSpan.FromMilliseconds(31);
            _timer.Tick += OnTimerTick;
            
            EndGame();
        }

        public void StartGame()
        {
            Paused = false;
            foreach (FrameworkElement invader in _invaders.Values)
                _sprites.Remove(invader);
            foreach (FrameworkElement shot in _shots.Values)
                _sprites.Remove(shot);
            _model.StartGame();
            OnPropertyChanged(nameof(GameOver));
            OnPropertyChanged(nameof(Victory));
            _timer.Start();
        }

        public void PauseGame()
        {
            if (GameOver) return;
            Paused = true;
            OnPropertyChanged(nameof(Paused));
        }
        
        public void ResumeGame()
        {
            if (GameOver) return;
            Paused = false;
            OnPropertyChanged(nameof(Paused));
        }

        private void EndGame()
        {
            _model.EndGame();
        }

        public void KeyDown(Key eKey)
        {
            switch (eKey)
            {
                case Key.Enter:
                    if (GameOver) StartGame();
                    if (Paused) ResumeGame();
                    break;
                case Key.Escape:
                    if (Paused) ResumeGame();
                    else PauseGame();
                    break;
                case Key.Space:
                    _model.FireShot();
                    break;
                case Key.A:
                case Key.Left:
                    _leftAction = DateTime.Now;
                    break;
                case Key.D:
                case Key.Right:
                    _rightAction = DateTime.Now;
                    break;
            }
        }

        public void KeyUp(Key eKey)
        {
            if (eKey == Key.Left || eKey == Key.A)
                _leftAction = null;
            if (eKey == Key.Right || eKey == Key.D)
                _rightAction = null;
        }

        public void LeftGestureStarted()
        {
            _leftAction = DateTime.Now;
        }

        public void RightGestureStarted()
        {
            _rightAction = DateTime.Now;
        }

        public void LeftGestureCompleted()
        {
            _leftAction = null;
        }

        public void RightGestureCompleted()
        {
            _rightAction = null;
        }

        public void Tapped()
        {
            _model.FireShot();
            // TODO: don't fire a shot when Start button is tapped
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnModelShipChanged(object? sender, ShipChangedEventArgs e)
        {
            if (!e.Killed)
            {
                if (e.Ship is Invader invader)
                {
                    if (!_invaders.ContainsKey(invader))
                    {
                        Point invaderLocation = new(invader.Location.X, invader.Location.Y);
                        AnimatedImage newInvaderControl = InvadersHelper.InvaderControlFactory(invader.Type, invaderLocation, Scale);
                        _invaders.Add(invader, newInvaderControl);
                        _sprites.Add(newInvaderControl);
                    }
                    else
                    {
                        Point invaderLocation = new(invader.Location.X, invader.Location.Y);
                        InvadersHelper.SetCanvasLocation(_invaders[invader], invaderLocation, Scale);
                        InvadersHelper.ResizeElement(_invaders[invader], invader.Size.Width, invader.Size.Height, Scale);
                    }
                }
                else if (e.Ship is Player player)
                {
                    if (_playerControl == null)
                    {
                        _lastPlayerBatteryCharge = player.CurrentBatteryCharge;
                        Point playerLocation = new(player.Location.X, player.Location.Y);
                        _playerControl = InvadersHelper.PlayerControlFactory(player.CurrentBatteryCharge, playerLocation, Scale);
                        _sprites.Add(_playerControl);
                    }
                    else
                    {
                        Point playerLocation = new(player.Location.X, player.Location.Y);
                        if (player.CurrentBatteryCharge != _lastPlayerBatteryCharge)
                        {
                            _sprites.Remove(_playerControl);
                            _lastPlayerBatteryCharge = player.CurrentBatteryCharge;
                            _playerControl = InvadersHelper.PlayerControlFactory(player.CurrentBatteryCharge, playerLocation, Scale);
                            _sprites.Add(_playerControl);
                        }
                        InvadersHelper.SetCanvasLocation(_playerControl, playerLocation, Scale);
                        InvadersHelper.ResizeElement(_playerControl, player.Size.Width, player.Size.Height, Scale);
                    }
                    if (_playerHitAnimationInProgress)
                    {
                        _playerHitAnimationInProgress = false;
                        AnimatedImage playerAnimatedImage = _playerControl as AnimatedImage;
                        playerAnimatedImage?.StopFlashing();
                    }
                }
            }
            else
            {
                if (e.Ship is Invader invader)
                {
                    if (!_invaders.ContainsKey(invader)) return;
                    AnimatedImage invaderAnimatedImage = _invaders[invader] as AnimatedImage;
                    invaderAnimatedImage?.FadeOut();
                    _shotInvaders.Add(_invaders[invader], DateTime.Now);
                    _invaders.Remove(invader);
                    _audioPlaybackViewModel.LaserHitCommand.Execute(null);
                }
                else if (e.Ship is Player player)
                {
                    if (Lives.Count == 0 && !_playerHitAnimationInProgress)
                    {
                        AnimatedImage playerAnimatedImage = _playerControl as AnimatedImage;
                        playerAnimatedImage?.FadeOut();
                        _audioPlaybackViewModel.PlayerDeadCommand.Execute(null);
                    }
                    else if (!_playerHitAnimationInProgress)
                    {
                        AnimatedImage playerAnimatedImage = _playerControl as AnimatedImage;
                        playerAnimatedImage?.StartFlashing();
                        _playerHitAnimationInProgress = true;
                        _audioPlaybackViewModel.PlayerHitCommand.Execute(null);
                    }
                    Point playerLocation = new(player.Location.X, player.Location.Y);
                    InvadersHelper.SetCanvasLocation(_playerControl, playerLocation, Scale);
                    InvadersHelper.ResizeElement(_playerControl, player.Size.Width, player.Size.Height, Scale);
                }
            }
        }

        private void OnModelShotMoved(object? sender, ShotMovedEventArgs e)
        {
            if (!e.Disappeared)
            {
                if (!_shots.Keys.Contains(e.Shot))
                {
                    Point shotLocation = new(e.Shot.Location.X, e.Shot.Location.Y);
                    FrameworkElement shotControl = InvadersHelper.ShotFactory(shotLocation, Scale);
                    _shots.Add(e.Shot, shotControl);
                    _sprites.Add(shotControl);
                    if (e.Shot.Direction == Direction.Up)
                    {
                        _audioPlaybackViewModel.PlayerLaserShootCommand.Execute(null);
                    }
                }
                else
                {
                    FrameworkElement shotControl = _shots[e.Shot];
                    Point shotLocation = new(e.Shot.Location.X, e.Shot.Location.Y);
                    InvadersHelper.SetCanvasLocation(shotControl, shotLocation, Scale);
                    InvadersHelper.ResizeElement(shotControl, Shot.ShotSize.Width, Shot.ShotSize.Height, Scale);
                }
            }
            else
            {
                if (_shots.Keys.Contains(e.Shot))
                {
                    _sprites.Remove(_shots[e.Shot]);
                    _shots.Remove(e.Shot);
                }
            }
        }

        private void OnModelStarChanged(object? sender, StarChangedEventArgs e)
        {
            if (e.Disappeared && _stars.ContainsKey(e.Location))
            {
                StarControl star = _stars[e.Location] as StarControl;
                InvadersHelper.ScaleStar(star, Scale);
                star?.FadeOut();
                _fadedStars.Add(_stars[e.Location], DateTime.Now);
                _stars.Remove(e.Location);
            }
            else
            {
                if (!_stars.ContainsKey(e.Location))
                {
                    Point starLocation = new(e.Location.X, e.Location.Y);
                    StarControl newStarControl = InvadersHelper.StarControlFactory(starLocation, Scale);
                    _stars.Add(e.Location, newStarControl);
                    _sprites.Add(newStarControl);
                    newStarControl.FadeIn();
                }
                else
                {
                    FrameworkElement starControl = _stars[e.Location];
                    InvadersHelper.ScaleStar(starControl as StarControl, Scale);
                    Point starLocation = new(e.Location.X, e.Location.Y);
                    InvadersHelper.SetCanvasLocation(starControl, starLocation, Scale);
                }
            }
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _model.Update(Paused);
            
            if (_lastPaused != Paused)
            {
                OnPropertyChanged(nameof(Paused));
            }

            if (!Paused)
            {
                if (_leftAction > _rightAction || _leftAction.HasValue && !_rightAction.HasValue)
                {
                    _model.MovePlayer(Direction.Left);
                }
                else if (_leftAction < _rightAction || !_leftAction.HasValue && _rightAction.HasValue)
                {
                    _model.MovePlayer(Direction.Right);
                }
            }
            
            UpdateStats();
            UpdateLivesIndicator();

            foreach (FrameworkElement shotInvader in _shotInvaders.Keys.ToList())
            {
                TimeSpan timeSinceInvaderWasShot = DateTime.Now - _shotInvaders[shotInvader];
                if (timeSinceInvaderWasShot > TimeSpan.FromMilliseconds(500))
                {
                    _sprites.Remove(shotInvader);
                    _shotInvaders.Remove(shotInvader);
                }
            }

            foreach (FrameworkElement fadedStar in _fadedStars.Keys.ToList())
            {
                TimeSpan timeSinceStarFaded = DateTime.Now - _fadedStars[fadedStar];
                if (timeSinceStarFaded > TimeSpan.FromMilliseconds(500))
                {
                    _sprites.Remove(fadedStar);
                    _fadedStars.Remove(fadedStar);
                }
            }

            if (GameOver)
            {
                OnPropertyChanged(nameof(GameOver));
                OnPropertyChanged(nameof(Victory));
                _timer.Stop();
            }

            void UpdateStats()
            {
                if (Score != _model.Score)
                {
                    Score = _model.Score;
                    OnPropertyChanged(nameof(Score));
                }

                if (Wave != _model.Wave)
                {
                    Wave = _model.Wave;
                    OnPropertyChanged(nameof(Wave));
                }
            }

            void UpdateLivesIndicator()
            {
                if (Lives.Count > _model.Lives && _model.Lives >= 0)
                {
                    int livesToRemove = Lives.Count - _model.Lives;
                    for (int i = 1; i <= livesToRemove; i++)
                    {
                        _lives.RemoveAt(Lives.Count - 1);
                    }
                }
                else if (Lives.Count < _model.Lives)
                {
                    int livesToAdd = _model.Lives - Lives.Count;
                    for (int i = 1; i <= livesToAdd; i++)
                    {
                        _lives.Add(new object());
                    }
                }
            }
        }
    }
}