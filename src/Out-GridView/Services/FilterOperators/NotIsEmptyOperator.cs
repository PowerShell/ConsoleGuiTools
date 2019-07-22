using System;
namespace OutGridView.Services.FilterOperators
{
    public class NotIsEmptyOperator : IFilterOperator
    {
        public bool HasValue { get; } = true;
        public string Value { get; set; }
        public bool Execute(string input)
        {
            return !String.IsNullOrEmpty(Value);
        }
    }
}