// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
namespace OutGridView.Application.Services.FilterOperators
{
    public class NotIsEmptyOperator : IStringFilterOperator
    {
        public bool HasValue { get; } = false;
        public string Value { get; set; }
        public bool Execute(string input)
        {
            return !String.IsNullOrEmpty(Value);
        }
        public string GetPowerShellString()
        {
            return $"-NE \'\'";
        }
    }
}
