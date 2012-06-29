using System;
using System.Globalization;
using System.Windows.Data;

namespace TAlex.GameOfLife.Controls.Converters
{
    public class Int32ToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Double))
            {
                return (double)((int)value);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Int32))
            {
                return (int)((double)value);
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
