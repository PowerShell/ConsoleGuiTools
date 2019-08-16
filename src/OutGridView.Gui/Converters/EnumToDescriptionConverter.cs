// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace OutGridView.Application.Converters
{
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            var enumValue = value as Enum;

            return enumValue?.GetDescriptionFromEnumValue() ?? Avalonia.AvaloniaProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return value;
        }
    }
}
