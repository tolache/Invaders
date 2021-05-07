using System;
using System.ComponentModel;
using System.Windows.Input;
using NAudio.Extras;

namespace Invaders.ViewModel
{
    public sealed class AudioPlaybackViewModel : INotifyPropertyChanged, IDisposable
    {
        public ICommand PlayerLaserShootCommand { get; }
        public ICommand LaserHitCommand { get; }
        
        private AudioPlaybackEngine _engine;
        private readonly CachedSound _playerLaserShot = new(@"Assets\Sounds\playerLaserShot.wav");
        private readonly CachedSound _laserHit = new(@"Assets\Sounds\laserHit.wav");

        public AudioPlaybackViewModel()
        {
            _engine = new AudioPlaybackEngine();
            PlayerLaserShootCommand = new DelegateCommand(() => _engine.PlaySound(_playerLaserShot));
            LaserHitCommand = new DelegateCommand(() => _engine.PlaySound(_laserHit));
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _engine?.Dispose();
            _engine = null;
        }
    }
}