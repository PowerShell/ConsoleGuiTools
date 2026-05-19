// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.PowerShell.OutGridView.Models;
using Terminal.Gui.Views;

namespace Microsoft.PowerShell.ConsoleGuiTools;

/// <summary>
///     An <see cref="ITableSource" /> implementation that wraps the OutGridView <see cref="DataTable" /> model.
///     Supports thread-safe row addition for streaming pipeline support (Issue #209).
/// </summary>
internal sealed class OutGridViewDataSource : ITableSource
{
    private readonly List<DataTableColumn> _columns;
    private readonly Lock _lock = new();
    private readonly List<DataTableRow> _rows;

    /// <summary>
    ///     Creates an empty data source with the specified columns.
    /// </summary>
    public OutGridViewDataSource(List<DataTableColumn> columns)
    {
        _columns = columns;
        ColumnNames = columns.Select(c => c.Label).ToArray();
        _rows = new List<DataTableRow>();
    }

    /// <summary>
    ///     Creates a data source with pre-populated rows.
    /// </summary>
    public OutGridViewDataSource(List<DataTableColumn> columns, List<DataTableRow> rows)
    {
        _columns = columns;
        ColumnNames = columns.Select(c => c.Label).ToArray();
        _rows = new List<DataTableRow>(rows);
    }

    /// <inheritdoc />
    public string[] ColumnNames { get; }

    /// <inheritdoc />
    public int Columns => _columns.Count;

    /// <inheritdoc />
    public int Rows
    {
        get
        {
            lock (_lock)
            {
                return _rows.Count;
            }
        }
    }

    /// <inheritdoc />
    public object this[int row, int col]
    {
        get
        {
            lock (_lock)
            {
                if (row < 0 || row >= _rows.Count || col < 0 || col >= _columns.Count)
                    return string.Empty;

                var dataRow = _rows[row];
                var columnKey = _columns[col].ToString();
                return dataRow.Values.TryGetValue(columnKey, out var value)
                    ? value.DisplayValue
                    : string.Empty;
            }
        }
    }

    /// <summary>
    ///     Thread-safely adds a row to the data source.
    /// </summary>
    public void AddRow(DataTableRow row)
    {
        lock (_lock)
        {
            _rows.Add(row);
        }
    }

    /// <summary>
    ///     Gets the original object index for the specified row.
    /// </summary>
    public int GetOriginalObjectIndex(int row)
    {
        lock (_lock)
        {
            return row >= 0 && row < _rows.Count ? _rows[row].OriginalObjectIndex : -1;
        }
    }

    /// <summary>
    ///     Gets the column definitions.
    /// </summary>
    public List<DataTableColumn> GetColumns()
    {
        return _columns;
    }

    /// <summary>
    ///     Creates a new filtered data source containing only rows matching the regex pattern.
    ///     Returns itself if the pattern is empty (no copy needed).
    /// </summary>
    public OutGridViewDataSource Filter(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return this;

        var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        List<DataTableRow> allRows;
        lock (_lock)
        {
            allRows = new List<DataTableRow>(_rows);
        }

        var result = allRows.Where(r => RowMatchesFilter(r, regex, _columns)).ToList();

        return new OutGridViewDataSource(_columns, result);
    }

    /// <summary>
    ///     Creates a new data source with rows sorted by the specified column.
    ///     The sorted column header is decorated with ▲ (ascending) or ▼ (descending).
    /// </summary>
    public OutGridViewDataSource Sort(int columnIndex, bool descending)
    {
        if (columnIndex < 0 || columnIndex >= _columns.Count)
            return this;

        List<DataTableRow> allRows;
        lock (_lock)
        {
            allRows = new List<DataTableRow>(_rows);
        }

        var columnKey = _columns[columnIndex].ToString();

        var sorted = descending
            ? allRows.OrderByDescending(r => GetSortValue(r, columnKey))
            : allRows.OrderBy(r => GetSortValue(r, columnKey));

        var result = new OutGridViewDataSource(_columns, sorted.ToList());

        // Decorate the sorted column header with a direction glyph
        string glyph = descending ? " ▼" : " ▲";
        result.ColumnNames[columnIndex] = _columns[columnIndex].Label + glyph;

        return result;
    }

    private static IComparable GetSortValue(DataTableRow row, string columnKey)
    {
        if (!row.Values.TryGetValue(columnKey, out var value))
            return string.Empty;

        // DecimalValue already carries a proper numeric SortValue via IComparable
        if (value is DecimalValue)
            return value;

        // For StringValue, try numeric parsing as a convenience for untyped data
        if (double.TryParse(value.DisplayValue, out var numericValue))
            return numericValue;

        return value.DisplayValue;
    }

    private static bool RowMatchesFilter(DataTableRow row, Regex regex, List<DataTableColumn> columns)
    {
        foreach (var column in columns)
        {
            var columnKey = column.ToString();
            if (row.Values.TryGetValue(columnKey, out var value) &&
                regex.IsMatch(value.DisplayValue))
                return true;
        }

        return false;
    }
}