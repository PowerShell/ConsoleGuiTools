// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
namespace OutGridView.Application.Services.FilterOperators
{
    public class NotContainsOperator : IStringFilterOperator
    {
        public bool HasValue { get; } = true;
        public string Value { get; set; }
        public bool Execute(string input)
        {
            return !input.Contains(Value, StringComparison.CurrentCultureIgnoreCase);
        }
        public string GetPowerShellString()
        {
            return $"-NotContains \'{Value}\'";
        }
    }
}
