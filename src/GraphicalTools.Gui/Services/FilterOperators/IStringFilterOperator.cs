// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace GraphicalTools.Application.Services.FilterOperators
{
    public interface IStringFilterOperator : IFilterOperator
    {
        string Value { get; }
    }
}
