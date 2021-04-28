using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Invaders.View
{
    public partial class AnimatedImage : UserControl
    {
        private Storyboard _flashStoryboard;
            
        public AnimatedImage()
        {
            InitializeComponent();
            _flashStoryboard = FindResource("FlashStoryboard") as Storyboard;
        }

        public AnimatedImage(IEnumerable<string> imageNames, TimeSpan interval) : this()
        {
            StartAnimation(imageNames, interval);
        }

        public void InvaderShot()
        {
            Storyboard invaderShotStoryboard = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(500));
            Storyboard.SetTarget(animation, Image);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Image.OpacityProperty));
            invaderShotStoryboard.Children.Add(animation);
            invaderShotStoryboard.Begin();
        }

        public void StartFlashing()
        {
            _flashStoryboard.Begin();
        }

        public void StopFlashing()
        {
            _flashStoryboard.Stop();
        }

        private void StartAnimation(IEnumerable<string> imageNames, TimeSpan interval)
        {
            Storyboard storyboard = new Storyboard();
            ObjectAnimationUsingKeyFrames animation = new ObjectAnimationUsingKeyFrames();
            Storyboard.SetTarget(animation, Image);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Image.SourceProperty));
            
            TimeSpan currentInterval = TimeSpan.Zero;
            foreach (string imageName in imageNames)
            {
                ObjectKeyFrame keyFrame = new DiscreteObjectKeyFrame();
                keyFrame.Value = CreateImageFromAssets(imageName);
                keyFrame.KeyTime = currentInterval;
                animation.KeyFrames.Add(keyFrame);
                currentInterval += interval;
            }

            storyboard.AutoReverse = true;
            storyboard.RepeatBehavior = RepeatBehavior.Forever;
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private BitmapImage CreateImageFromAssets(string imageName)
        {
            Uri uri = new Uri("pack://application:,,,/Assets/Images/" + imageName, UriKind.Absolute);
            return new BitmapImage(uri);
        }
    }
}