using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace OutGridView.Converters
{


    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            var enumValue = value as Enum;

            return enumValue == null ? Avalonia.AvaloniaProperty.UnsetValue : enumValue.GetDescriptionFromEnumValue();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return value;
        }
    }
}