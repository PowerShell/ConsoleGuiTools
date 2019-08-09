// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace OutGridView.Application.Services.FilterOperators
{
    public class EqualsOperator : IStringFilterOperator
    {
        public bool HasValue { get; } = true;
        public string Value { get; set; }
        public bool Execute(string input)
        {
            return input.Equals(Value, StringComparison.CurrentCultureIgnoreCase);
        }
        public string GetPowerShellString()
        {
            return $"-EQ \'{Value}\'";
        }
    }
}
