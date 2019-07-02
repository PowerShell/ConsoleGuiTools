using System.Reflection;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Linq;
using System.Collections.Generic;

namespace OutGridView.Models
{
    public class Column : ReactiveObject
    {
        public PropertyInfo Property { get; set; }
        public string PropertyName => Property.Name;
        [Reactive] public Boolean IsVisible { get; set; }
        public Column(PropertyInfo _propertyInfo)
        {
            Property = _propertyInfo;
            IsVisible = true;
        }
    }
}