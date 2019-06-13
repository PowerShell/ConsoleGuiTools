using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace OutGridView.Services.CriteriaFilters
{
    public class ContainsRule : ICriteriaFilterRule
    {
        public string value { get; set; }
        public bool Execute(string input)
        {
            return input.Contains(value);
        }
    }
}