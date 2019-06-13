using System.Reflection;

namespace OutGridView.Models
{
    public class CriteriaCheckbox
    {
        public bool IsChecked { get; set; }
        public PropertyInfo Property { get; set; }

        public string PropertyName => Property.Name;

        public CriteriaCheckbox(PropertyInfo property)
        {
            IsChecked = false;
            Property = property;
        }
    }
}