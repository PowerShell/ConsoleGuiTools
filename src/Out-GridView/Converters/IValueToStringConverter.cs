using System;
using Avalonia.Data.Converters;
using OutGridView.Models;
using System.Globalization;

namespace OutGridView.Converters
{
    public class IValueToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            IValue stringValue = value as IValue;
            return stringValue.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return value;
        }
    }
}