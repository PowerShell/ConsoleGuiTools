// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;

namespace OutGridView.Application.Models
{
    public enum StringFilterOperator
    {
        [Description("Contains")]
        Contains,
        [Description("Equals")]
        Equals,
        [Description("Doesn't Contain")]
        NotContains,
        [Description("Starts With")]
        StartsWith,
        [Description("Doesn't Equal")]
        NotEquals,
        [Description("Ends With")]
        EndwsWith,
        [Description("Is Not Empty")]
        NotIsEmpty,
        [Description("Is Empty")]
        IsEmpty
    }
}
