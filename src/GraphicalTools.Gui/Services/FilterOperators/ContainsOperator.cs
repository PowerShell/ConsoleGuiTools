// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace OutGridView.Application.Services.FilterOperators
{
    public class ContainsOperator : IStringFilterOperator
    {
        public bool HasValue { get; } = true;
        public string Value { get; set; }
        public bool Execute(string input)
        {
            return input.Contains(Value, StringComparison.CurrentCultureIgnoreCase);
        }
        public string GetPowerShellString()
        {
            var val = PowerShellCodeGenerator.EscapePowerShellLikeString(Value);
            return $"-Like \'*{val}*\'";
        }
    }
}
