using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace OutGridView.Services.FilterOperators
{
    public class ContainsOperator : IFilterOperatorLookup
    {
        public string Value { get; set; }
        public bool Execute(string input)
        {
            return input.Contains(Value);
        }
    }
}