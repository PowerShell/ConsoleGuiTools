using System.Reflection;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace OutGridView.Models
{
    public class CriteriaFilter : ReactiveObject
    {
        public PropertyInfo Property { get; set; }
        public CriteriaFilterType Type { get; set; }
        public string PropertyName => Property.Name;
        [Reactive] public string Value { get; set; }
        public CriteriaFilter(PropertyInfo _propertyInfo)
        {
            Property = _propertyInfo;
            Type = CriteriaFilterType.Contains;
            Value = string.Empty;
        }
    }
}