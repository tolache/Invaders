using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Invaders.View
{
    public partial class StarControl : UserControl
    {
        private readonly Storyboard _fadeInStoryboard;
        private readonly Storyboard _fadeOutStoryboard;

        public StarControl()
        {
            InitializeComponent();
            _fadeInStoryboard = (Storyboard) FindResource("FadeInStoryboard");
            _fadeOutStoryboard = (Storyboard) FindResource("FadeOutStoryboard");
        }

        public void FadeIn()
        {
            _fadeInStoryboard.Begin();
        }
        
        public void FadeOut()
        {
            _fadeOutStoryboard.Begin();
        }
    }
}