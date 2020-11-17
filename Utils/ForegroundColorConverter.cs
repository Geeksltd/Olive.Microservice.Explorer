using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MicroserviceExplorer
{
    public class ForegroundColorConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
                if (typeof(Color) == targetType)
                    return brush.Color;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}