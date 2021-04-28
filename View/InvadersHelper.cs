using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Invaders.Model;

namespace Invaders.View
{
    public static class InvadersHelper
    {
        private static readonly Random _random = new Random();

        public static AnimatedImage InvaderControlFactory(InvaderType type, double x, double y, double scale)
        {
            List<string> imageNames = new List<string>
            {
                type + "1.png",
                type + "2.png",
                type + "3.png",
                type + "4.png",
            };
            AnimatedImage invader = new AnimatedImage(imageNames, TimeSpan.FromMilliseconds(100));
            ResizeElement(invader, Invader.InvaderSize.Width, Invader.InvaderSize.Height, scale);
            SetCanvasLocation(invader, x, y, scale);
            return invader;
        }
        
        public static AnimatedImage PlayerControlFactory(double x, double y, double scale)
        {
            List<string> imageNames = new List<string>
            {
                "player.png",
            };
            AnimatedImage player = new AnimatedImage(imageNames, TimeSpan.FromMilliseconds(1000));
            ResizeElement(player, Player.PlayerSize.Width, Player.PlayerSize.Height, scale);
            SetCanvasLocation(player, x, y, scale);
            return player;
        }
        
        public static FrameworkElement ShotFactory(double x, double y, double scale)
        {
            Rectangle shot = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Yellow),
                Width = Shot.ShotSize.Width,
                Height = Shot.ShotSize.Height,
            };
            ResizeElement(shot, shot.Width, shot.Height, scale);
            SetCanvasLocation(shot, x, y, scale);
            return shot;
        }

        public static Rectangle ScanLineFactory(double y, double scale)
        {
            Rectangle scanLine = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.LightBlue), 
                Width = 400 * scale,
                Height = 2 * scale, 
                Opacity = .1,
            };
            SetCanvasLocation(scanLine,1.25, y, scale);
            BringToFront(scanLine);
            return scanLine;
        }

        public static StarControl StarControlFactory(double x, double y, double scale)
        {
            StarControl star = new();
            int randomInt = _random.Next(3);
            switch (randomInt)
            {
                case 0:
                    star.Style = (Style) Application.Current.Resources["BigStar"];
                    break;
                case 1:
                    star.Style = (Style) Application.Current.Resources["RectangleStar"];
                    break;
                case 2:
                    star.Style = (Style) Application.Current.Resources["EllipseStar"];
                    break;
                default:
                    star.Style = (Style) Application.Current.Resources["BigStar"];
                    break;
            }

            star.Foreground = GetRandomStarColor();
            SetCanvasLocation(star, x, y, scale);
            ScaleStar(star, scale);
            SendToBack(star);
            return star;
        }
        
        public static void ScaleStar(StarControl star, double scale)
        {
            const double scaleCoefficient = 0.5;
            star.RenderTransformOrigin = new Point(0.5, 0.5);
            star.ScaleTransform.ScaleX = scale * scaleCoefficient;
            star.ScaleTransform.ScaleY = scale * scaleCoefficient;
        }

        private static SolidColorBrush GetRandomStarColor()
        {
            switch (_random.Next(7))
            {
                case 0:
                    return new SolidColorBrush(Colors.OrangeRed);
                case 1:
                    return new SolidColorBrush(Colors.Orange);
                case 2:
                    return new SolidColorBrush(Colors.Yellow);
                case 3:
                    return new SolidColorBrush(Colors.White);
                case 4:
                    return new SolidColorBrush(Colors.LightBlue);
                case 5:
                    return new SolidColorBrush(Colors.DodgerBlue);
                case 6:
                    return new SolidColorBrush(Colors.MediumBlue);
                default:
                    return new SolidColorBrush(Colors.White);
            }
        }

        public static void SetCanvasLocation(UIElement control, double x, double y, double scale)
        {
            Canvas.SetLeft(control, x * scale);
            Canvas.SetTop(control, y * scale);
        }

        public static void ResizeElement(FrameworkElement control, double width, double height, double scale)
        {
            control.Width = width * scale;
            control.Height = height * scale;
        }

        public static void BringToFront(UIElement uiElement)
        {
            Panel.SetZIndex(uiElement, 1000);
        }

        private static void SendToBack(UIElement uiElement)
        {
            Panel.SetZIndex(uiElement, -1000);
        }
    }
}