// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace OutGridView.Cmdlet
{
    public class GridViewRow
    {
        public string DisplayString { get; set; }
        public bool IsMarked { get; set; }
        public int OriginalIndex { get; set; }
        public override string ToString() => DisplayString;
    }
}
