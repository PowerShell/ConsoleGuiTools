// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using OutGridView.Application.Models;
using System;

namespace OutGridView.Application.Services.FilterOperators
{
    public static class FilterOperatorLookup
    {
        public static IStringFilterOperator CreateFilterOperatorRule(StringFilterOperator filterOp, string value)
        {
            switch (filterOp)
            {
                case StringFilterOperator.Contains:
                    return new ContainsOperator { Value = value };
                case StringFilterOperator.Equals:
                    return new EqualsOperator { Value = value };
                case StringFilterOperator.NotContains:
                    return new NotContainsOperator { Value = value };
                case StringFilterOperator.StartsWith:
                    return new StartsWithOperator { Value = value };
                case StringFilterOperator.EndwsWith:
                    return new EndsWithOperator { Value = value };
                case StringFilterOperator.NotEquals:
                    return new NotEqualsOperator { Value = value };
                case StringFilterOperator.NotIsEmpty:
                    return new NotIsEmptyOperator { Value = value };
                case StringFilterOperator.IsEmpty:
                    return new IsEmptyOperator();
                default:
                    throw new Exception("Invalid Rule");
            }
        }
    }
}
