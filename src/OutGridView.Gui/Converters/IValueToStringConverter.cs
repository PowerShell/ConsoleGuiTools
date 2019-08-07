using System;
using Avalonia.Data.Converters;
using OutGridView.Application.Models;
using System.Globalization;
using OutGridView.Models;

namespace OutGridView.Application.Converters
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