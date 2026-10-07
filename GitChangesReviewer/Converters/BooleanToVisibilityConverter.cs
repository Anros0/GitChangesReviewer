using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GitChangesReviewer.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;
            return GetVisibility(boolValue, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool boolValue = visibility == Visibility.Visible;
                return ApplyInversion(boolValue, parameter);
            }
            return false;
        }

        private Visibility GetVisibility(bool value, object parameter)
        {
            bool finalValue = ApplyInversion(value, parameter);
            return finalValue ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool ApplyInversion(bool value, object parameter)
        {
            if (parameter is string paramStr && bool.TryParse(paramStr, out bool invert) && invert)
            {
                return !value;
            }
            return value;
        }
    }
}
