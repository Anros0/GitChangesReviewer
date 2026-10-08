using System.Globalization;
using System.Windows.Data;

namespace GitChangesReviewer.Converters
{
    internal class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                if (parameter is string format && !string.IsNullOrWhiteSpace(format))
                    return timeSpan.ToString(format, culture);

                return timeSpan.ToString(@"hh\:mm\:ss", culture);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
