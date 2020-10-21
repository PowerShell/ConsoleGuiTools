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
        public static List<GridViewRow> FilterData(List<GridViewRow> list, string filter)
        {
            var items = new List<GridViewRow>();
            if (string.IsNullOrEmpty(filter))
            {
                filter = ".*";
            }

            foreach (GridViewRow gvr in list)
            {
                if(Regex.IsMatch(gvr.DisplayString, filter, RegexOptions.IgnoreCase))
                {
                    items.Add(gvr);
                }
            }

            return items;
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
