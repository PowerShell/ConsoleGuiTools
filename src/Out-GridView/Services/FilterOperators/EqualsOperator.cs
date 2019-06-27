using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace OutGridView.Services.FilterOperators
{
    public class EqualsOperator : IFilterOperatorLookup
    {
        public string Value { get; set; }
        public bool Execute(string input)
        {
            return input.Equals(Value);
        }
    }
}