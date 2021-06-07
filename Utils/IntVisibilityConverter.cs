using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MicroserviceExplorer
{
	public class IntVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string ValStr = value as string;
			if (int.TryParse(ValStr, out int Val))
			{
				return Val == 0 ? Visibility.Collapsed : Visibility.Visible;
			}
			else
			{
				return value;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	public class BoolVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool val)) return value;

			return val ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}