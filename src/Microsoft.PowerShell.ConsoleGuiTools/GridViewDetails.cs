// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace OutGridView.Cmdlet
{
    internal class GridViewDetails
    {
        // Contains the width of each column in the grid view.
        public int[] ListViewColumnWidths { get; set; }

        // Dictates where the header should actually start considering
        // some offset is needed to factor in the checkboxes
        public int ListViewOffset { get; set; }

        // The width that is actually useable on the screen after
        // subtracting space needed for a clean UI (spaces between columns, etc).
        public int UsableWidth { get; set; }
    }
}
