// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace OutGridView.Application.Services.FilterOperators
{
    public interface IFilterOperator
    {
        bool HasValue { get; }
        bool Execute(string input);
        string GetPowerShellString();
    }
}
