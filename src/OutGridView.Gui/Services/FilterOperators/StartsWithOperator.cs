// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Globalization;
namespace OutGridView.Application.Services.FilterOperators
{
    public class StartsWithOperator : IStringFilterOperator
    {
        public bool HasValue { get; } = true;
        public string Value { get; set; }
        public bool Execute(string input)
        {
            return input.StartsWith(Value, true, CultureInfo.CurrentCulture);
        }
        public string GetPowerShellString()
        {
            var val = PowerShellCodeGenerator.EscapePowerShellLikeString(Value);
            return $"-Like \'{val}*\'";
        }
    }
}
