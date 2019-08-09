// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace OutGridView.Application.Services.FilterOperators
{
    public abstract class NumericFilterOperator : IFilterOperator
    {
        public bool HasValue { get; } = true;
        public decimal Value { get; set; }
        public abstract bool Execute(string input);
        public abstract string GetPowerShellString();
    }
}
