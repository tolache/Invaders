using System;
using System.ComponentModel;
using System.Windows.Input;
using NAudio.Extras;

namespace Invaders.ViewModel
{
    public class AudioPlaybackViewModel : INotifyPropertyChanged, IDisposable
    {
        public ICommand PlayerLaserShootCommand { get; }
        
        private AudioPlaybackEngine _engine;
        private readonly CachedSound _playerLaserShot = new("Assets\\Sounds\\playerLaserShot.wav");

        public AudioPlaybackViewModel()
        {
            _engine = new AudioPlaybackEngine();
            PlayerLaserShootCommand = new DelegateCommand(() => _engine.PlaySound(_playerLaserShot));
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
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