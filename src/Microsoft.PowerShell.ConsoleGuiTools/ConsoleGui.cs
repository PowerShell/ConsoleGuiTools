// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NStack;
using OutGridView.Models;
using Terminal.Gui;

namespace OutGridView.Cmdlet
{
    internal class ConsoleGui : IDisposable
    {
        private const string FILTER_LABEL = "Filter";
        // This adjusts the left margin of all controls
        private const int MARGIN_LEFT = 2;
        // Width of Terminal.Gui ListView selection/check UI elements (old == 4, new == 2)
        private const int CHECK_WIDTH = 4;
        private bool _cancelled;
        private GridViewDataSource _itemSource;
        private Label _filterLabel;
        private TextField _filterField;
        private ListView _listView;
        private ApplicationData _applicationData;
        private GridViewDetails _gridViewDetails;

        public HashSet<int> Start(ApplicationData applicationData)
        {
            Application.Init();
            _applicationData = applicationData;
            _gridViewDetails = new GridViewDetails
            {
                // If OutputMode is Single or Multiple, then we make items selectable. If we make them selectable,
                // 2 columns are required for the check/selection indicator and space.
                ListViewOffset = _applicationData.OutputMode != OutputModeOption.None ? MARGIN_LEFT + CHECK_WIDTH : MARGIN_LEFT
            };

            Window win = AddTopLevelWindow();
            AddStatusBar();

            // GridView header logic
            List<string> gridHeaders = _applicationData.DataTable.DataColumns.Select((c) => c.Label).ToList();
            CalculateColumnWidths(gridHeaders);

            AddFilter(win);
            AddHeaders(win, gridHeaders);

            // GridView row logic
            LoadData();
            AddRows(win);

            _filterField.Text = _applicationData.Filter ?? string.Empty;
            _filterField.CursorPosition = _filterField.Text.Length;
            // Run the GUI.
            Application.Run();
            Application.Shutdown();

            // Return results of selection if required.
            HashSet<int> selectedIndexes = new HashSet<int>();
            if (_cancelled)
            {
                return selectedIndexes;
            }

            foreach (GridViewRow gvr in _itemSource.GridViewRowList)
            {
                if (gvr.IsMarked)
                {
                    selectedIndexes.Add(gvr.OriginalIndex);
                }
            }

            return selectedIndexes;
        }

        private void Accept()
        {
            Application.RequestStop();
        }

        private void Close()
        {
            _cancelled = true;
            Application.RequestStop();
        }

        private Window AddTopLevelWindow()
        {
            // Creates the top-level window to show
            var win = new Window(_applicationData.Title)
            {
                X = 0,
                Y = 0,
                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Fill(1)
            };

            Application.Top.Add(win);
            return win;
        }

        private void AddStatusBar()
        {
            var statusBar = new StatusBar(
                    _applicationData.OutputMode != OutputModeOption.None
                    ? new StatusItem[]
                    {
                        // Use Key.Unknown for SPACE with no delegate because ListView already
                        // handles SPACE
                        new StatusItem(Key.Unknown, "~SPACE~ Mark Item", null),
                        new StatusItem(Key.Enter, "~ENTER~ Accept", () =>
                        {
                            if (Application.Top.MostFocused == _listView)
                            {
                                // If nothing was explicitly marked, we return the item that was selected
                                // when ENTER is pressed in Single mode. If something was previously selected
                                // (using SPACE) then honor that as the single item to return
                                if (_applicationData.OutputMode == OutputModeOption.Single &&
                                    _itemSource.GridViewRowList.Find(i => i.IsMarked) == null)
                                {
                                    _listView.MarkUnmarkRow();
                                }
                                Accept();
                            }
                            else if (Application.Top.MostFocused == _filterField)
                            {
                                _listView.SetFocus();
                            }
                        }),
                        new StatusItem(Key.Esc, "~ESC~ Close", () => Close())
                    }
                    : new StatusItem[]
                    {
                        new StatusItem(Key.Esc, "~ESC~ Close",  () => Close())
                    }
            );

            Application.Top.Add(statusBar);
        }

        private void CalculateColumnWidths(List<string> gridHeaders)
        {
            _gridViewDetails.ListViewColumnWidths = new int[gridHeaders.Count];
            var listViewColumnWidths = _gridViewDetails.ListViewColumnWidths;

            for (int i = 0; i < gridHeaders.Count; i++)
            {
                listViewColumnWidths[i] = gridHeaders[i].Length;
            }

            // calculate the width of each column based on longest string in each column for each row
            foreach (var row in _applicationData.DataTable.Data)
            {
                int index = 0;

                // use half of the visible buffer height for the number of objects to inspect to calculate widths
                foreach (var col in row.Values.Take(Application.Top.Frame.Height / 2))
                {
                    var len = col.Value.DisplayValue.Length;
                    if (len > listViewColumnWidths[index])
                    {
                        listViewColumnWidths[index] = len;
                    }

                    index++;
                }
            }

            // if the total width is wider than the usable width, remove 1 from widest column until it fits
            _gridViewDetails.UsableWidth = Application.Top.Frame.Width - MARGIN_LEFT - listViewColumnWidths.Length - _gridViewDetails.ListViewOffset;
            int columnWidthsSum = listViewColumnWidths.Sum();
            while (columnWidthsSum >= _gridViewDetails.UsableWidth)
            {
                int maxWidth = 0;
                int maxIndex = 0;
                for (int i = 0; i < listViewColumnWidths.Length; i++)
                {
                    if (listViewColumnWidths[i] > maxWidth)
                    {
                        maxWidth = listViewColumnWidths[i];
                        maxIndex = i;
                    }
                }

                listViewColumnWidths[maxIndex]--;
                columnWidthsSum--;
            }
        }

        private void AddFilter(Window win)
        {
            _filterLabel = new Label(FILTER_LABEL)
            {
                X = MARGIN_LEFT
            };

            _filterField = new TextField(string.Empty)
            {
                X = Pos.Right(_filterLabel) + 1,
                Y = Pos.Top(_filterLabel),
                CanFocus = true,
                Width = Dim.Fill() - _filterLabel.Text.Length
            };

            var filterErrorLabel = new Label(string.Empty)
            {
                X = Pos.Right(_filterLabel) + 1,
                Y = Pos.Top(_filterLabel) + 1,
                ColorScheme = Colors.Base,
                Width = Dim.Fill() - _filterLabel.Text.Length
            };

            _filterField.TextChanged += (str) =>
            {
                // str is the OLD value
                string filterText = _filterField.Text?.ToString();
                try
                {
                    filterErrorLabel.Text = " ";
                    filterErrorLabel.ColorScheme = Colors.Base;
                    filterErrorLabel.Redraw(filterErrorLabel.Bounds);

                    List<GridViewRow> itemList = GridViewHelpers.FilterData(_itemSource.GridViewRowList, filterText);
                    _listView.Source = new GridViewDataSource(itemList);
                }
                catch (Exception ex)
                {
                    filterErrorLabel.Text = ex.Message;
                    filterErrorLabel.ColorScheme = Colors.Error;
                    filterErrorLabel.Redraw(filterErrorLabel.Bounds);
                    _listView.Source = _itemSource;
                }
            };

            win.Add(_filterLabel, _filterField, filterErrorLabel);
        }

        private void AddHeaders(Window win, List<string> gridHeaders)
        {
            var header = new Label(GridViewHelpers.GetPaddedString(
                gridHeaders,
                _gridViewDetails.ListViewOffset,
                _gridViewDetails.ListViewColumnWidths))
            {
                X = 0,
                Y = 2
            };

            win.Add(header);

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
        }

        private void LoadData()
        {
            var items = new List<GridViewRow>();
            int newIndex = 0;
            for (int i = 0; i < _applicationData.DataTable.Data.Count; i++)
            {
                var dataTableRow = _applicationData.DataTable.Data[i];
                var valueList = new List<string>();
                foreach (var dataTableColumn in _applicationData.DataTable.DataColumns)
                {
                    string dataValue = dataTableRow.Values[dataTableColumn.ToString()].DisplayValue;
                    valueList.Add(dataValue);
                }

                string displayString = GridViewHelpers.GetPaddedString(valueList, 0, _gridViewDetails.ListViewColumnWidths);

                items.Add(new GridViewRow
                {
                    DisplayString = displayString,
                    OriginalIndex = i
                });

                newIndex++;
            }

            _itemSource = new GridViewDataSource(items);
        }

        private void AddRows(Window win)
        {
            _listView = new ListView(_itemSource)
            {
                X = Pos.Left(_filterLabel),
                Y = Pos.Bottom(_filterLabel) + 3, // 1 for space, 1 for header, 1 for header underline
                Width = Dim.Fill(2),
                Height = Dim.Fill(),
                AllowsMarking = _applicationData.OutputMode != OutputModeOption.None,
                AllowsMultipleSelection = _applicationData.OutputMode == OutputModeOption.Multiple,
            };

            win.Add(_listView);
        }

        public void Dispose()
        {
            if (!Console.IsInputRedirected)
            {
                // By emitting this, we fix two issues:
                // 1. An issue where arrow keys don't work in the console because .NET
                //    requires application mode to support Arrow key escape sequences.
                //    Esc[?1h sets the cursor key to application mode
                //    See http://ascii-table.com/ansi-escape-sequences-vt-100.php
                // 2. An issue where moving the mouse causes characters to show up because
                //    mouse tracking is still on. Esc[?1003l turns it off.
                //    See https://www.xfree86.org/current/ctlseqs.html#Mouse%20Tracking
                Console.Write("\u001b[?1h\u001b[?1003l");
            }
        }
    }
}
