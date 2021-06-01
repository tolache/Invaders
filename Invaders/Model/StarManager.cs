using System;
using System.Collections.Generic;
using System.Drawing;

namespace Invaders.Model
{
    public class StarManager
    {
        private readonly List<Point> _stars = new();
        private const int InitialStarCount = 50;
        private readonly OnStarChanged _onStarChanged;
        private readonly Random _random = new();

        public delegate void OnStarChanged(Point location, bool disappeared);

        public StarManager(OnStarChanged onStarChanged)
        {
            _onStarChanged = onStarChanged;
        }
        
        public void RecreateStars()
        {
            foreach (var star in _stars)
            {
                _onStarChanged(star, true);
            }

            _stars.Clear();
            
            for (int i = 0; i < InitialStarCount; i++)
            {
                CreateStar();
            }
        }
        
        private void CreateStar()
        {
            int s = _random.Next(5, 20);
            Size size = new Size(s, s);
            int x = _random.Next(0, InvadersModel.PlayAreaSize.Width - size.Width);
            int y = _random.Next(0, InvadersModel.PlayAreaSize.Height - size.Height);
            Point location = new Point(x, y);
            _stars.Add(location);
            _onStarChanged(location, false);
        }
        
        public void Twinkle()
        {
            if (_stars.Count < InitialStarCount * 0.85 ||
                _stars.Count >= InitialStarCount * 0.85 && _stars.Count < InitialStarCount * 1.5 &&
                _random.Next(0, 2) == 1)
            {
                CreateStar();
            }
            else
            {
                RemoveStar();
            }
            
            return;

            void RemoveStar()
            {
                int index = _random.Next(0, _stars.Count);
                Point star = _stars[index];
                _stars.RemoveAt(index);
                _onStarChanged(star, true);
            }
        }
        
        public void UpdateAllStars()
        {
            foreach (Point star in _stars)
            {
                _onStarChanged(star, false);
            }
        }
    }
}