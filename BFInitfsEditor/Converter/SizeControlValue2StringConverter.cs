using System;
using System.Globalization;
using System.Windows.Data;

namespace BFInitfsEditor.Converter
{
    public class SizeControlValue2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sizeControlValue = (int) value;

            return sizeControlValue <= 0
                ? $"{sizeControlValue}" // negative or neutral value
                : $"+{sizeControlValue}"; // positive value
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}