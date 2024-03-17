// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace OutGridView.Cmdlet.TreeNodeCaching
{
    internal interface ICachedMemberResult
    {
        bool IsCollection { get; }
        public object Value {get;}

        public IReadOnlyCollection<CachedMemberResultElement> Elements {get;}
    }
}
