using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace OutGridView.Services.CriteriaFilters
{
    public interface ICriteriaFilterRule
    {
        string value { get; set; }
        bool Execute(string input);
    }
}