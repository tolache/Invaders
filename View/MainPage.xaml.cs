using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Invaders.ViewModel;

namespace Invaders.View
{
    public partial class MainPage : Page
    {
        private readonly InvadersViewModel _invadersViewModel;
        private bool _lastGestureIsTap = true;

        public MainPage()
        {
            InitializeComponent();
            _invadersViewModel = FindResource(nameof(InvadersViewModel)) as InvadersViewModel;
            InvadersHelper.BringToFront(GameOverText);
            InvadersHelper.BringToFront(PausedText);
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.KeyDown += OnKeyDown;
                window.KeyUp += OnKeyUp;
            }
        }

        private void MainPage_OnUnloaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.KeyDown -= OnKeyDown;
                window.KeyUp -= OnKeyUp;
            }
        }

        private void PlayAreaBorder_OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdatePlayAreaSize(new Size(MainGrid.RenderSize.Width, PlayAreaRow.ActualHeight));
        }

        private void MainGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePlayAreaSize(new Size(e.NewSize.Width, PlayAreaRow.ActualHeight));
        }

        private void PlayAreaBorder_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePlayAreaSize(new Size(MainGrid.RenderSize.Width, PlayAreaRow.ActualHeight));
        }

        private void UpdatePlayAreaSize(Size newGridSize)
        {
            double targetWidth;
            double targetHeight;
            if (newGridSize.Width / newGridSize.Height > (double)4 / 3)
            {
                targetWidth = newGridSize.Height * 4 / 3;
                targetHeight = newGridSize.Height;
                double leftRightMargin = (newGridSize.Width - targetWidth) / 2;
                StatsPanel.Margin = new Thickness(0, 0, leftRightMargin, 0);
                PlayAreaBorder.Margin = new Thickness(leftRightMargin, 0, leftRightMargin, 0);
            }
            else
            {
                targetHeight = newGridSize.Width * 3 / 4;
                targetWidth = newGridSize.Width;
                double topBottomMargin = (newGridSize.Height - targetHeight) / 2;
                StatsPanel.Margin = new Thickness(0, topBottomMargin, 0, 0);
                PlayAreaBorder.Margin = new Thickness(0, 0, 0, topBottomMargin);
            }

            PlayAreaBorder.Width = targetWidth;
            PlayAreaBorder.Height = targetHeight;
            GameOverText.Margin = new Thickness(0, PlayAreaBorder.Height * 0.4, 0, 0);
            PausedText.Margin = new Thickness(0, PlayAreaBorder.Height * 0.4, 0, 0);
            _invadersViewModel.PlayAreaSize = PlayAreaBorder.RenderSize;
        }

        private void MainPage_OnManipulationDelta(object? sender, ManipulationDeltaEventArgs e)
        {
            if (e.DeltaManipulation.Translation.X < -1)
            {
                _lastGestureIsTap = false;
                _invadersViewModel.LeftGestureStarted();
            }
            else if (e.DeltaManipulation.Translation.X > 1)
            {
                _lastGestureIsTap = false;
                _invadersViewModel.RightGestureStarted();
            }
        }

        private void MainPage_OnManipulationCompleted(object? sender, ManipulationCompletedEventArgs e)
        {
            _invadersViewModel.LeftGestureCompleted();
            _invadersViewModel.RightGestureCompleted();
        }

        private void MainPage_OnManipulationStarted(object? sender, ManipulationStartedEventArgs e)
        {
            
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            _invadersViewModel.KeyDown(e.Key);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            _invadersViewModel.KeyUp(e.Key);
        }

        private void MainPage_OnTouchLeave(object? sender, TouchEventArgs e)
        {
            if (_lastGestureIsTap)
            {
                _invadersViewModel.Tapped();
            }

            _lastGestureIsTap = true;
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            _invadersViewModel.StartGame();
        }

        private void AboutButton_OnClick(object sender, RoutedEventArgs e)
        {
            BitmapImage appBi = new BitmapImage(new System.Uri("pack://application:,,,/Assets/logo-large.png"));
            BitmapImage cBi = new BitmapImage(new System.Uri("pack://application:,,,/Assets/tolache-logo.png"));

            AboutControlView about = new AboutControlView();
            AboutControlViewModel vm = (AboutControlViewModel)about.FindResource("ViewModel");
            vm.IsSemanticVersioning = true;
            vm.ApplicationLogo = appBi;
            vm.PublisherLogo = cBi;
            vm.HyperlinkText = "https://github.com/tolache";

            vm.Window.Content = about;
            vm.Window.Show();
        }
    }
}