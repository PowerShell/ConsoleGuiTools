// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
            return stringValue.DisplayValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return value;
        }
    }
}
