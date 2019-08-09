using System;
namespace OutGridView.Application.Services.FilterOperators
{
    public class IsEmptyOperator : IStringFilterOperator
    {
        public bool HasValue { get; } = false;
        public string Value { get; set; }
        public bool Execute(string input)
        {
            return String.IsNullOrWhiteSpace(input);
        }
        public string GetPowerShellString()
        {
            return $"-EQ \'\'";
        }
    }
}