// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NStack;
using OutGridView.Models;
using Terminal.Gui;

namespace OutGridView.Cmdlet
{
    internal class GridViewHelpers
    {
        // Add all items already selected plus any that match the filter
        // The selected items should be at the top of the list, in their original order
        public static List<GridViewRow> FilterData(List<GridViewRow> listToFilter, string filter)
        {
            var filteredList = new List<GridViewRow>();
            if (string.IsNullOrEmpty(filter))
            {
                return listToFilter;
            }

            filteredList.AddRange(listToFilter.Where(gvr => gvr.IsMarked));
            filteredList.AddRange(listToFilter.Where(gvr => !gvr.IsMarked && Regex.IsMatch(gvr.DisplayString, filter, RegexOptions.IgnoreCase)));

            return filteredList;
        }

        public static string GetPaddedString(List<string> strings, int offset, int[] listViewColumnWidths)
        {
            var builder = new StringBuilder();
            if (offset > 0)
            {
                builder.Append(string.Empty.PadRight(offset));
            }

            for (int i = 0; i < strings.Count; i++)
            {
                if (i > 0)
                {
                    // Add a space between columns
                    builder.Append(' ');
                }

                // Replace any newlines with encoded newline/linefeed (`n or `r)
                // Note we can't use Environment.Newline because we don't know that the
                // Command honors that.
                strings[i] = strings[i].Replace("\r", "`r");
                strings[i] = strings[i].Replace("\n", "`n");

                // If the string won't fit in the column, append an ellipsis.
                if (strings[i].Length > listViewColumnWidths[i])
                {
                    builder.Append(strings[i], 0, listViewColumnWidths[i] - 3);
                    builder.Append("...");
                }
                else
                {
                    builder.Append(strings[i].PadRight(listViewColumnWidths[i]));
                }
            }

            return builder.ToString();
        }
    }
}
