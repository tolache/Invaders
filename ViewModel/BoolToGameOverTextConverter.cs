using System;
using System.Globalization;
using System.Windows.Data;

namespace Invaders.ViewModel
{
    public class BoolToGameOverTextConverter : IValueConverter

    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is true ? "VICTORY" : "GAME OVER";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}