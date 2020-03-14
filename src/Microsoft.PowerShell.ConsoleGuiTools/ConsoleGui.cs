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
    internal class ConsoleGui : IDisposable
    {
        private const string ACCEPT_TEXT = "Are you sure you want to select\nthese items to send down the pipeline?";
        private const string CANCEL_TEXT = "Are you sure you want to cancel?\nNothing will be emitted to the pipeline.";
        private const string CLOSE_TEXT = "Are you sure you want to close?";
        private bool _cancelled;
        private List<string> _itemList;
        private ListView _list;

        internal HashSet<int> SelectedIndexes { get; private set; } = new HashSet<int>();
        public void Start(ApplicationData applicationData)
        {
            Application.Init();
            var top = Application.Top;

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
                        new MenuItem("_Accept", string.Empty, () => { if (Quit("Accept", ACCEPT_TEXT)) Application.RequestStop(); }),
                        new MenuItem("_Cancel", string.Empty, () =>{ if (Quit("Cancel", CANCEL_TEXT)) _cancelled = true; Application.RequestStop(); })
                    }
                    : new MenuItem []
                    {
                        new MenuItem("_Close", string.Empty, () =>{ if (Quit("Close", CLOSE_TEXT)) Application.RequestStop(); })
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

                // use half of the visible buffer height for the number of objects to inspect to calculate widths
                foreach (var col in row.Values.Take(top.Frame.Height / 2))
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

            var filterLabel = new Label("Filter")
            {
                X = 2
            };

            // 7 here is to reserve space for the Apply button
            var filterFieldWidth = usableWidth - filterLabel.Text.Length - 7;
            var filterField = new TextField(".*")
            {
                X = Pos.Right(filterLabel) + 1,
                Y = Pos.Top(filterLabel),
                CanFocus = true,
                Width = filterFieldWidth
            };
            filterField.Changed += FilterField_Changed;

            var filterError = new Label(string.Empty)
            {
                X = Pos.Right(filterLabel) + 1,
                Y = Pos.Top(filterLabel) + 1,
                TextColor = Terminal.Gui.Attribute.Make(Color.Red, Color.Blue), // How to get window background color?
                Width = filterFieldWidth
            };

            var filterApplyButton = new Button("Apply")
            {
                // Pos.Right(filterField) returns 0
                X = Pos.Right(filterLabel) + 2 + filterFieldWidth,
                Y = Pos.Top(filterLabel),
                Clicked = () =>
                {
                    FilterData(applicationData, filterField.Text.ToString(), columnWidths, offset, filterError);
                    _list.SetSource(_itemList);
                }
            };

            var header = new Label(GetPaddedString(gridHeaders, columnWidths, offset + offset - 1))
            {
                X = 0,
                Y = 2
            };

            win.Add(filterLabel, filterField, filterError, filterApplyButton, header);
            var headerLineText = new StringBuilder();
            foreach (char c in header.Text)
            {
                if (c.Equals(' '))
                {
                    headerLineText.Append(' ');
                }
                else
                {
                    // When gui.cs supports text decorations, should replace this with just underlining the header
                    headerLineText.Append('-');
                }
            }

            var headerLine = new Label(headerLineText.ToString())
            {
                X = 0,
                Y = 3
            };
            win.Add(headerLine);

            FilterData(applicationData, ".*", columnWidths, offset, filterError);

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

            _list = new ListView(_itemList)
            {
                X = 3,
                Y = 4,
                Width = Dim.Fill(2),
                Height = Dim.Fill(2),
                AllowsMarking = applicationData.PassThru
            };

            win.Add(_list);

            Application.Run();

            if (_cancelled)
            {
                return;
            }

            for (int i = 0; i < applicationData.DataTable.Data.Count; i++)
            {
                if(_list.Source.IsMarked(i))
                {
                    SelectedIndexes.Add(i);
                }
            }
        }

        private static bool Quit(string title, string text)
        {
            var n = MessageBox.Query(50, 7, title, text, "Yes", "No");
            return n == 0;
        }

        private void FilterData(ApplicationData applicationData, string filter, int[] columnWidths, int offset, Label filterError)
        {
            var items = new List<string>();
            filterError.Text = " ";
            filterError.TextColor = Terminal.Gui.Attribute.Make(Color.Red, Color.Blue);  // How to get window background color?
            filterError.Redraw(filterError.Bounds);

            foreach (DataTableRow dataTableRow in applicationData.DataTable.Data)
            {
                bool match = false;
                var valueList = new List<string>();
                foreach (var dataTableColumn in applicationData.DataTable.DataColumns)
                {
                    string dataValue = dataTableRow.Values[dataTableColumn.ToString()].DisplayValue;

                    if (!match)
                    {
                        try
                        {
                            if (Regex.IsMatch(dataValue, filter, RegexOptions.IgnoreCase))
                            {
                                match = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            filterError.Text = ex.Message;  
                            filterError.TextColor = Terminal.Gui.Attribute.Make(Color.Red, Color.Black);
                            filterError.Redraw(filterError.Bounds);
                            return;
                        }
                    }
                    
                    valueList.Add(dataValue);
                }

                if (match)
                {
                    items.Add(GetPaddedString(valueList, columnWidths, offset));
                }
            }

            _itemList = items;
        }

        private void FilterField_Changed(object sender, ustring e)
        {
            // this doesn't get hit when field text is changed?
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
                    // Add a space between columns
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

        public void Dispose()
        {
            // By emitting this, we fix an issue where arrow keys don't work in the console
            // because .NET requires application mode to support Arrow key escape sequences
            // Esc[?1h - Set cursor key to application mode
            // See http://ascii-table.com/ansi-escape-sequences-vt-100.php
            Console.Write("\u001b[?1h");
        }
    }
}
