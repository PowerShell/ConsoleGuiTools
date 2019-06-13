using OutGridView.Models;
using System;
using System.Collections.Generic;

namespace OutGridView.Services.CriteriaFilters
{
    public static class CriteriaFilterLookup
    {
        public static ICriteriaFilterRule CreateCriteriaFilterRule(CriteriaFilter filter)
        {
            switch (filter.Type)
            {
                case CriteriaFilterType.Contains:
                    return new ContainsRule { value = filter.Value };
                case CriteriaFilterType.Equals:
                    return new EqualsRule { value = filter.Value };
                default:
                    throw new Exception("Invalid Rule");
            }
        }
    }
}