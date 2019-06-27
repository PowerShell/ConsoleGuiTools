using OutGridView.Models;
using System;

namespace OutGridView.Services.FilterOperators
{
    public static class FilterOperatorLookup
    {
        public static IFilterOperatorLookup CreateFilterOperatorRule(Filter filter)
        {
            switch (filter.Operator)
            {
                case FilterOperator.Contains:
                    return new ContainsOperator { Value = filter.Value };
                case FilterOperator.Equals:
                    return new EqualsOperator { Value = filter.Value };
                default:
                    throw new Exception("Invalid Rule");
            }
        }
    }
}