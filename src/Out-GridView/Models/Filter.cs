using System.Reflection;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Linq;
using System.Collections.Generic;

namespace OutGridView.Models
{
    public class Filter : ReactiveObject
    {
        public static IEnumerable<FilterOperator> Operators { get; } = Enum.GetValues(typeof(FilterOperator)).Cast<FilterOperator>();
        public PropertyInfo Property { get; set; }
        [Reactive] public FilterOperator Operator { get; set; }
        [Reactive] public string Value { get; set; }
        public Filter(PropertyInfo _propertyInfo)
        {
            Property = _propertyInfo;
            Operator = FilterOperator.Contains;
            Value = string.Empty;
        }
    }
}