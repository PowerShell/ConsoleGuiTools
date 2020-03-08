// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using OutGridView.Models;
using Terminal.Gui;

namespace OutGridView.Cmdlet
{
    internal class ConsoleGui
    {
        private const string ACCEPT_TEXT = "Are you sure you want to select\nthese items to send down the pipeline?";
        private const string CANCEL_TEXT = "Are you sure you want to cancel?\nNothing will be emitted to the pipeline.";
        private const string CLOSE_TEXT = "Are you sure you want to close?";

        private bool _cancelled;

        internal HashSet<int> SelectedIndexes { get; private set; } = new HashSet<int>();
        public void Start(ApplicationData applicationData)
        {
            Application.Init();
            var top = Application.Top;

            // Since top is static, state would be preserved in a PowerShell process
            // so we remove everything from Top as a precaution.
            top.RemoveAll();

            // Creates the top-level window to show
            var win = new Window(applicationData.Title ?? "Out-ConsoleGridView")
            {
                X = 0,
                Y = 1, // Leave one row for the toplevel menu
                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            top.Add(win);

            // Creates a menubar, the item "New" has a help menu.
            var menu = new MenuBar(new MenuBarItem []
            {
                new MenuBarItem("_Actions (F9)", 
                    applicationData.PassThru
                    ? new MenuItem []
                    {
                        new MenuItem("_Accept", "", () => { if (Quit("Accept", ACCEPT_TEXT)) top.Running = false; }),
                        new MenuItem("_Cancel", "", () =>{ if (Quit("Cancel", CANCEL_TEXT)) _cancelled = true; top.Running = false; })
                    }
                    : new MenuItem []
                    {
                        new MenuItem("_Close", "", () =>{ if (Quit("Close", CLOSE_TEXT)) top.Running = false; })
                    })
            });
            top.Add(menu);

            var gridHeaders = applicationData.DataTable.DataColumns.Select((c) => c.Label).ToList();
            var columnWidths = new int[gridHeaders.Count];
            for (int i = 0; i < gridHeaders.Count; i++)
            {
                columnWidths[i] = gridHeaders[i].Length;
            }

            // calculate the width of each column based on longest string in each column for each row
            foreach (var row in applicationData.DataTable.Data)
            {
                int index = 0;
                // use just the first 20 rows to calculate in case there is a large dataset
                foreach (var col in row.Values.Take(20))
                {
                    var len = col.Value.DisplayValue.Length;
                    if (len > columnWidths[index])
                    {
                        columnWidths[index] = len;
                    }
                    
                    index++;
                }
            }

            // If we have PassThru, then we want to make them selectable. If we make them selectable,
            // they have a 8 character addition of a checkbox ("     [ ]") that we have to factor in.
            int offset = applicationData.PassThru ? 8 : 4;

            // if the total width is wider than the usable width, remove 1 from widest column until it fits
            // the gui loses 3 chars on the left and 2 chars on the right
            int usableWidth = top.Frame.Width - 3 - columnWidths.Length - offset - 2;
            int columnWidthsSum = columnWidths.Sum();
            while (columnWidthsSum >= usableWidth)
            {
                int maxWidth = 0;
                int maxIndex = 0;
                for (int i = 0; i < columnWidths.Length; i++)
                {
                    if (columnWidths[i] > maxWidth)
                    {
                        maxWidth = columnWidths[i];
                        maxIndex = i;
                    }
                }

                columnWidths[maxIndex]--;
                columnWidthsSum--;
            }

            win.Add(new Label(GetPaddedString(gridHeaders, columnWidths, offset + offset - 1)));

            var items = new List<string>();
            foreach (DataTableRow dataTableRow in applicationData.DataTable.Data)
            {
                var valueList = new List<string>();
                foreach (var dataTableColumn in applicationData.DataTable.DataColumns)
                {
                    valueList.Add(dataTableRow.Values[dataTableColumn.ToString()].DisplayValue);
                }

                items.Add(GetPaddedString(valueList, columnWidths, offset));
            }

            var list = new ListView(items)
            {
                X = 3,
                Y = 3,
                Width = Dim.Fill(2),
                Height = Dim.Fill(2),
                AllowsMarking = applicationData.PassThru
            };
            
            win.Add(list);

            Application.Run();

            if (_cancelled)
            {
                return;
            }

            for (int i = 0; i < applicationData.DataTable.Data.Count; i++)
            {
                if(list.Source.IsMarked(i))
                {
                    SelectedIndexes.Add(i);
                }
            }
        }

        public void Close()
        {
            // top.Running = false;
        }

        private static bool Quit(string title, string text)
        {
            var n = MessageBox.Query(50, 7, title, text, "Yes", "No");
            return n == 0;
        }

        private static string GetPaddedString(List<string> strings, int[] colWidths, int offset = 0)
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
                    builder.Append(' ');
                }

                // If the string won't fit in the column, append an ellipsis.
                if (strings[i].Length > colWidths[i])
                {
                    builder.Append(strings[i].Substring(0, colWidths[i] - 4));
                    builder.Append("...");
                }
                else
                {
                    builder.Append(strings[i].PadRight(colWidths[i]));
                }
            }

            return builder.ToString();
        }
    }
}
