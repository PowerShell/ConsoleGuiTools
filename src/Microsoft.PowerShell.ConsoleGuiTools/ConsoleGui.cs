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
        private const string FILTER_LABEL = "Filter";
        private const string APPLY_LABEL = "Apply";
        private bool _cancelled;
        private List<string> _itemList;
        private ListView _listView;
        private ApplicationData _applicationData;
        private int[] _listViewColumnWidths;
        private int _listViewOffset;
        private Label _filterErrorLabel;

        internal HashSet<int> SelectedIndexes { get; private set; } = new HashSet<int>();
        public void Start(ApplicationData applicationData)
        {
            Application.Init();
            var top = Application.Top;
            _applicationData = applicationData;

            // If we have PassThru, then we want to make them selectable. If we make them selectable,
            // they have a 8 character addition of a checkbox ("     [ ]") that we have to factor in.
            _listViewOffset = _applicationData.PassThru ? 8 : 4;

            // Creates the top-level window to show
            var win = new Window(_applicationData.Title)
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
                    _applicationData.PassThru
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

            var gridHeaders = _applicationData.DataTable.DataColumns.Select((c) => c.Label).ToList();
            _listViewColumnWidths = new int[gridHeaders.Count];
            for (int i = 0; i < gridHeaders.Count; i++)
            {
                _listViewColumnWidths[i] = gridHeaders[i].Length;
            }

            // calculate the width of each column based on longest string in each column for each row
            foreach (var row in _applicationData.DataTable.Data)
            {
                int index = 0;

                // use half of the visible buffer height for the number of objects to inspect to calculate widths
                foreach (var col in row.Values.Take(top.Frame.Height / 2))
                {
                    var len = col.Value.DisplayValue.Length;
                    if (len > _listViewColumnWidths[index])
                    {
                        _listViewColumnWidths[index] = len;
                    }
                    
                    index++;
                }
            }

            // if the total width is wider than the usable width, remove 1 from widest column until it fits
            // the gui loses 3 chars on the left and 2 chars on the right
            int usableWidth = top.Frame.Width - 3 - _listViewColumnWidths.Length - _listViewOffset - 2;
            int columnWidthsSum = _listViewColumnWidths.Sum();
            while (columnWidthsSum >= usableWidth)
            {
                int maxWidth = 0;
                int maxIndex = 0;
                for (int i = 0; i < _listViewColumnWidths.Length; i++)
                {
                    if (_listViewColumnWidths[i] > maxWidth)
                    {
                        maxWidth = _listViewColumnWidths[i];
                        maxIndex = i;
                    }
                }

                _listViewColumnWidths[maxIndex]--;
                columnWidthsSum--;
            }

            var filterLabel = new Label(FILTER_LABEL)
            {
                X = 2
            };

            // 2 is for the square brackets added to buttons
            var filterFieldWidth = usableWidth - filterLabel.Text.Length - APPLY_LABEL.Length - 2;
            var filterField = new TextField(string.Empty)
            {
                X = Pos.Right(filterLabel) + 1,
                Y = Pos.Top(filterLabel),
                CanFocus = true,
                Width = filterFieldWidth
            };
            filterField.Changed += FilterField_Changed;

            _filterErrorLabel = new Label(string.Empty)
            {
                X = Pos.Right(filterLabel) + 1,
                Y = Pos.Top(filterLabel) + 1,
                ColorScheme = Colors.Base,
                Width = filterFieldWidth
            };

            var filterApplyButton = new Button(APPLY_LABEL)
            {
                // Pos.Right(filterField) returns 0
                X = Pos.Right(filterLabel) + filterFieldWidth + 2,
                Y = Pos.Top(filterLabel),
                Clicked = () =>
                {
                    FilterData(filterField.Text.ToString());
                    _listView.SetSource(_itemList);
                }
            };

            var header = new Label(GetPaddedString(gridHeaders, _listViewOffset + _listViewOffset - 1))
            {
                X = 0,
                Y = 2
            };

            win.Add(filterLabel, filterField, _filterErrorLabel, filterApplyButton, header);

            // This renders dashes under the header to make it more clear what is header and what is data
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

            FilterData(string.Empty);

            var items = new List<string>();
            foreach (DataTableRow dataTableRow in _applicationData.DataTable.Data)
            {
                var valueList = new List<string>();
                foreach (var dataTableColumn in _applicationData.DataTable.DataColumns)
                {
                    valueList.Add(dataTableRow.Values[dataTableColumn.ToString()].DisplayValue);
                }

                items.Add(GetPaddedString(valueList));
            }

            _listView = new ListView(_itemList)
            {
                X = 3,
                Y = 4,
                Width = Dim.Fill(2),
                Height = Dim.Fill(2),
                AllowsMarking = _applicationData.PassThru
            };

            win.Add(_listView);

            Application.Run();

            if (_cancelled)
            {
                return;
            }

            for (int i = 0; i < _applicationData.DataTable.Data.Count; i++)
            {
                if(_listView.Source.IsMarked(i))
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

        private void FilterData(string filter)
        {
            var items = new List<string>();
            if (string.IsNullOrEmpty(filter))
            {
                filter = ".*";
            }

            _filterErrorLabel.Text = " ";
            _filterErrorLabel.ColorScheme = Colors.Base;
            _filterErrorLabel.Redraw(_filterErrorLabel.Bounds);

            foreach (DataTableRow dataTableRow in _applicationData.DataTable.Data)
            {
                bool match = false;
                var valueList = new List<string>();
                foreach (var dataTableColumn in _applicationData.DataTable.DataColumns)
                {
                    string dataValue = dataTableRow.Values[dataTableColumn.ToString()].DisplayValue;

                    try
                    {
                        if (!match && Regex.IsMatch(dataValue, filter, RegexOptions.IgnoreCase))
                        {
                            match = true;
                            // don't break out of loop as all data need to be collected to be rendered
                        }
                    }
                    catch (Exception ex)
                    {
                        _filterErrorLabel.Text = ex.Message;
                        _filterErrorLabel.ColorScheme = Colors.Error;
                        _filterErrorLabel.Redraw(_filterErrorLabel.Bounds);
                        return;
                    }
                    
                    valueList.Add(dataValue);
                }

                if (match)
                {
                    items.Add(GetPaddedString(valueList));
                }
            }

            _itemList = items;
        }

        private void FilterField_Changed(object sender, ustring e)
        {
            // TODO: remove Apply button and code when this starts working
            FilterData(e.ToString());
            _listView.SetSource(_itemList);
        }

        private string GetPaddedString(List<string> strings)
        {
            return GetPaddedString(strings, _listViewOffset);
        }

        private string GetPaddedString(List<string> strings, int offset)
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
                if (strings[i].Length > _listViewColumnWidths[i])
                {
                    builder.Append(strings[i].Substring(0, _listViewColumnWidths[i] - 4));
                    builder.Append("...");
                }
                else
                {
                    builder.Append(strings[i].PadRight(_listViewColumnWidths[i]));
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
