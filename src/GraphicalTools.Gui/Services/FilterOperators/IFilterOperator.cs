// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace GraphicalTools.Application.Services.FilterOperators
{
    public interface IFilterOperator
    {
        bool HasValue { get; }
        bool Execute(string input);
        string GetPowerShellString();
    }
}
