// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.PowerShell.OutGridView.Models;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Microsoft.PowerShell.ConsoleGuiTools;

/// <summary>
///     Provides the Terminal.Gui Window implementation for displaying tabular data using <see cref="TableView" />
///     with filtering, marking, and streaming support.
/// </summary>
internal sealed class OutGridViewWindow : Runnable<HashSet<int>>
{
    private const string FILTER_LABEL = "_Filter:";

    private readonly ApplicationData _applicationData;
    private readonly OutGridViewDataSource _masterDataSource;

    private OutGridViewDataSource? _filteredDataSource;
    private View? _filterErrorView;
    private TextField? _filterField;
    private Label? _filterLabel;
    private bool _isLoading = true;
    private int _sortColumn = -1;
    private bool _sortDescending;

    private StatusBar? _statusBar;
    private TableView? _tableView;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OutGridViewWindow" /> class.
    /// </summary>
    /// <param name="applicationData">The configuration and data to display.</param>
    /// <param name="dataSource">The data source (may grow during streaming).</param>
    public OutGridViewWindow(ApplicationData applicationData, OutGridViewDataSource dataSource)
    {
        _applicationData = applicationData;
        _masterDataSource = dataSource;

        Title = _applicationData.Title ?? "Out-ConsoleGridView";
        SchemeName = SchemeManager.SchemesToSchemeName(Schemes.Base);
        BorderStyle = FrameView.DefaultBorderStyle;

        switch (_applicationData.MinUI)
        {
            case true:
                //BorderStyle = LineStyle.None;
                Border.Thickness = new Thickness(0, string.IsNullOrEmpty(_applicationData.Title) ? 0 : 1, 0, 0);
                if (!string.IsNullOrEmpty(_applicationData.Filter)) AddFilter();
                break;
            case false:
                Border.Thickness = new Thickness(0, 2, 0, 0);
                AddFilter();
                break;
        }

        AddStatusBar();
        AddTableView();
        ApplyFilter();
    }

    /// <summary>
    ///     Optional callback invoked when the window starts running (UI is ready for interaction).
    /// </summary>
    internal Action? OnRunning { get; set; }

    private int _maxHeight;
    private bool _laidOut;

    protected override void OnIsRunningChanged(bool newIsRunning)
    {
        base.OnIsRunningChanged(newIsRunning);
        if (!newIsRunning) return;

        App?.LayoutAndDrawComplete += (_, _) =>
        {
            _maxHeight = !_laidOut ? Frame.Height : Math.Max(_maxHeight, Frame.Height);
            _laidOut = true;
        };

        if (App?.AppModel == AppModel.Inline && Height.Has(out DimFill _))
        {
            // If starting inline and height is Dim.Fill, change to Dim.Auto to avoid full screen
            Height = Dim.Auto();

            _tableView!.Height = Dim.Auto(
                minimumContentDim:
                Dim.Func(_ => Math.Max(
                    _filteredDataSource?.Rows ?? 0,
                    _maxHeight - ((_tableView.FrameToScreen().Top + _tableView.GetAdornmentsThickness().Vertical) +
                                  (_statusBar?.Frame.Height ?? 0) + Border.Thickness.Bottom))),
                maximumContentDim:
                Dim.Func(_ =>
                    App?.Driver?.Screen.Height -
                    ((_tableView.FrameToScreen().Top + _tableView.GetAdornmentsThickness().Vertical) +
                     (_statusBar?.Frame.Height ?? 0) + Border.Thickness.Bottom) ?? 0));
        }

        OnRunning?.Invoke();

        if (_applicationData.Focus == FocusTarget.Filter && _filterField != null)
            _filterField.SetFocus();
        else
            _tableView?.SetFocus();
    }

    /// <summary>
    ///     Called (via Application.Invoke) when new rows have been added to the master data source during streaming.
    /// </summary>
    public void OnDataChanged()
    {
        if (string.IsNullOrEmpty(_applicationData.Filter))
        {
            // No filter active — just point the table at the master source directly (no copy)
            if (_filteredDataSource != _masterDataSource)
            {
                _filteredDataSource = _masterDataSource;
                _tableView!.Table = _filteredDataSource;
            }
            _tableView?.Update();
        }
        else
        {
            ApplyFilter();
        }

        if (_isLoading)
            _rowsShortcut.Text = $"{_masterDataSource.Rows} rows";
    }

    /// <summary>
    ///     Called (via Application.Invoke) when the pipeline has finished sending objects.
    /// </summary>
    public void OnPipelineComplete()
    {
        _isLoading = false;
        OnLoadingComplete();
        ApplySearch();
    }

    /// <summary>
    ///     Gets the original indexes of all selected rows using TableView's native multi-selection.
    /// </summary>
    public HashSet<int> GetSelectedIndexes()
    {
        if (_tableView == null || _filteredDataSource == null)
            return [];

        var selectedRows = new HashSet<int>();
        foreach (var cell in _tableView.GetAllSelectedCells())
        {
            var origIdx = _filteredDataSource.GetOriginalObjectIndex(cell.Y);
            if (origIdx >= 0)
                selectedRows.Add(origIdx);
        }

        return selectedRows;
    }

    #region User Actions

    private void Accept()
    {
        Result = GetSelectedIndexes();
        App?.RequestStop();
    }

    #endregion

    #region Filtering

    private void ApplyFilter()
    {
        // Save the currently selected row's original index so we can restore position
        int? selectedOriginalIndex = null;
        if (_filteredDataSource != null && _tableView is { Value.SelectedCell.Y: >= 0 } &&
            _tableView.Value.SelectedCell.Y < _filteredDataSource.Rows)
            selectedOriginalIndex = _filteredDataSource.GetOriginalObjectIndex(_tableView.Value.SelectedCell.Y);

        try
        {
            if (_filterErrorView != null) _filterErrorView.Text = string.Empty;
            _filteredDataSource = _masterDataSource.Filter(_applicationData.Filter ?? string.Empty);
        }
        catch (RegexParseException ex)
        {
            if (_filterErrorView != null) _filterErrorView.Text = ex.Message;
            return;
        }

        RebuildTableSource();

        // Restore selection position
        if (selectedOriginalIndex.HasValue && _filteredDataSource != null && _tableView != null)
            for (var i = 0; i < _filteredDataSource.Rows; i++)
                if (_filteredDataSource.GetOriginalObjectIndex(i) == selectedOriginalIndex.Value)
                {
                    _tableView.SetSelection(0, i, false);
                    break;
                }

        _tableView?.Update();
    }

    private void RebuildTableSource()
    {
        if (_filteredDataSource == null || _tableView == null) return;
        _tableView.Table = _filteredDataSource;
    }

    private void ApplySortAndFilter()
    {
        // Apply filter first, then sort
        ApplyFilter();

        if (_sortColumn < 0 || _filteredDataSource == null || _tableView == null)
            return;

        _filteredDataSource = _filteredDataSource.Sort(_sortColumn, _sortDescending);
        _tableView.Table = _filteredDataSource;
        _tableView?.Update();
    }

    private void ApplySearch()
    {
        if (string.IsNullOrEmpty(_applicationData.Search) || _filteredDataSource == null || _tableView == null)
            return;

        try
        {
            var regex = new Regex(_applicationData.Search, RegexOptions.IgnoreCase);
            for (var row = 0; row < _filteredDataSource.Rows; row++)
            {
                for (var col = 0; col < _filteredDataSource.Columns; col++)
                {
                    var cellValue = _filteredDataSource[row, col].ToString() ?? string.Empty;
                    if (regex.IsMatch(cellValue))
                    {
                        _tableView.SetSelection(0, row, false);
                        return;
                    }
                }
            }
        }
        catch (RegexParseException)
        {
            // Invalid regex — silently ignore
        }
    }

    #endregion

    #region UI Construction

    private void AddFilter()
    {
        _filterLabel = new Label
        {
            Text = FILTER_LABEL,
            X = 0,
            Y = 0
        };

        _filterField = new TextField
        {
            Text = _applicationData.Filter ?? string.Empty,
            X = Pos.Right(_filterLabel) + 1,
            Y = Pos.Top(_filterLabel),
            CanFocus = true,
            Width = Dim.Fill() - 1
        };

        _filterField.KeyBindings.Remove(Key.A.WithCtrl);

        _filterField.Accepted += (_, _) =>
        {
            if (_applicationData.OutputMode != OutputModeOption.None)
                Accept();
        };

        _filterErrorView = new View
        {
            Text = string.Empty,
            X = Pos.Right(_filterLabel) + 1,
            Y = Pos.Top(_filterLabel) + 1,
            Width = Dim.Fill() - _filterLabel.Text.Length,
            Height = Dim.Auto(DimAutoStyle.Text),
            SchemeName = SchemeManager.SchemesToSchemeName(Schemes.Error)
        };

        _filterField.TextChanged += (_, _) =>
        {
            try
            {
                _filterErrorView?.Text = string.Empty;
                _applicationData.Filter = _filterField.Text;
                ApplyFilter();
            }
            catch (Exception ex)
            {
                _filterErrorView?.Text = ex.Message;
            }
        };

        Add(_filterLabel, _filterField, _filterErrorView);
        _filterField.Text = _applicationData.Filter ?? string.Empty;
        _filterField.InsertionPoint = _filterField.Text.Length;
    }

    private void AddTableView()
    {
        _tableView = new TableView
        {
            X = 0,
            Y = _filterErrorView is not null ? Pos.Bottom(_filterErrorView) : 0,
            Width = Dim.Fill(),
            Height = _statusBar is not null ? Dim.Fill(_statusBar) : Dim.Fill(),
            FullRowSelect = true,
            MultiSelect = _applicationData.OutputMode == OutputModeOption.Multiple,
            Style = new TableStyle
            {
                ShowHeaders = true,
                AlwaysShowHeaders = true,
                ExpandLastColumn = true,
                ShowHorizontalHeaderUnderline = false,
                ShowVerticalHeaderLines = false,
                ShowHorizontalHeaderOverline = false,
                ShowVerticalCellLines = false,
                SmoothHorizontalScrolling = true
            },
            ViewportSettings = ViewportSettingsFlags.HasScrollBars
        };

        // TableView typically is a grid where nav keys are biased for moving left/right.
        _tableView.KeyBindings.Remove(Key.Home);
        _tableView.KeyBindings.Add(Key.Home, Command.Start);
        _tableView.KeyBindings.Remove(Key.End);
        _tableView.KeyBindings.Add(Key.End, Command.End);

        // Enter key activates selection
        if (_applicationData.OutputMode != OutputModeOption.None) _tableView.Accepted += (_, _) => Accept();

        // Column header click sorts
        _tableView.Activating += (_, e) =>
        {
            if (e.Context?.Binding is not MouseBinding { MouseEvent: { } mouse })
                return;

            if (!mouse.Flags.HasFlag(MouseFlags.LeftButtonClicked))
                return;

            _tableView.ScreenToCell(mouse.Position!.Value, out int? clickedCol);
            if (clickedCol == null)
                return;

            // Toggle direction if clicking same column, otherwise sort ascending
            if (clickedCol.Value == _sortColumn)
                _sortDescending = !_sortDescending;
            else
            {
                _sortColumn = clickedCol.Value;
                _sortDescending = false;
            }

            ApplySortAndFilter();
        };

        Add(_tableView);
    }

    private SpinnerView? _spinnerView;

    private readonly Shortcut _rowsShortcut = new()
    {
        Text = "Rows:",
        CanFocus = false,
        MouseHighlightStates = MouseState.None
    };

    private void AddStatusBar()
    {
        if (_applicationData.MinUI) return;

        var shortcuts = new List<Shortcut>();

        // Spinner as 1st item while loading (streaming or reloading)
        _spinnerView = new() { Style = new SpinnerStyle.Aesthetic(), Width = 8 };
        _spinnerView.AutoSpin = _isLoading;
        _rowsShortcut.CommandView = _spinnerView;
        shortcuts.Add(_rowsShortcut);

        if (_applicationData.OutputMode != OutputModeOption.None)
            shortcuts.Add(new Shortcut(Key.Enter, "Accept", null));

        if (_applicationData.OutputMode == OutputModeOption.Multiple)
        {
            shortcuts.Add(new Shortcut(Key.A.WithCtrl, "Sel. All", () => _tableView?.SelectAll()));
            shortcuts.Add(new Shortcut(Key.D.WithCtrl, "Desel. All", () =>
            {
                _tableView?.MultiSelectedRegions.Clear();
                _tableView?.SetNeedsDraw();
            }));
        }

        shortcuts.Add(new Shortcut(Key.Esc, "Close", RequestStop));

        if (_applicationData.Verbose || _applicationData.Debug)
        {
            shortcuts.Add(new Shortcut(Key.Empty, $" v{_applicationData.ModuleVersion}", null));
            var tgFileVersionInfo =
                FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(Application))!.Location);
            var tgVersion = tgFileVersionInfo.FileVersion ?? "no version found";
            {
                var plusIdx = tgFileVersionInfo.ProductVersion?.IndexOf('+') ?? -1;
                tgVersion = plusIdx >= 0
                    ? tgFileVersionInfo.ProductVersion![..plusIdx]
                    : tgFileVersionInfo.ProductVersion ?? tgVersion;
            }
            shortcuts.Add(new Shortcut(Key.Empty, $"{App?.Driver?.GetName()} v{tgVersion}", null));
        }

        _statusBar = new StatusBar(shortcuts);
        Add(_statusBar);
        MoveSubViewToEnd(_statusBar);
    }

    private void OnLoadingComplete()
    {
        if (_applicationData.MinUI) return;
        if (_statusBar == null) return;

        // Stop spinner and replace with final row count
        // Setting CommandView disposes the old view
        _spinnerView = null;
        _rowsShortcut.CommandView = new View(){
            Width = Dim.Auto(),
            Height = Dim.Fill()
        };

        _rowsShortcut.Title = $"{_masterDataSource.Rows}";
        _rowsShortcut.Text = "Rows: ";
    }

    #endregion
}