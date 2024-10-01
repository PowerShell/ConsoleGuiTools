// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.PowerShell.ConsoleGuiTools.Models;

namespace Microsoft.PowerShell.ConsoleGuiTools;

internal static class GridViewHelpers
{
    // Add all items already selected plus any that match the filter
    // The selected items should be at the top of the list, in their original order
    internal static List<GridViewRow> FilterData(List<GridViewRow> listToFilter, string filter)
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

    /// <summary>
    /// Creates the header and data source for the GridView.
    /// </summary>
    /// <param name="listViewOffset"> Dictates where the header should actually start considering
    /// some offset is needed to factor in the checkboxes
    /// </param>
    /// <param name="applicationData"></param>
    /// <param name="leftMargin">Dictates where the header should actually start considering some offset is needed to factor in the checkboxes</param>
    /// <returns><see cref="GridViewHeader"/> and <see cref="GridViewDataSource"/> from commandlet inputs.</returns>
    internal static (GridViewHeader Header, GridViewDataSource DataSource) CreateGridViewInputs(int listViewOffset, int leftMargin, ApplicationData applicationData, object[] properties)
    {
        var table = FormatHelper.FormatTable(applicationData.Input, applicationData.Force, properties);

        var gridViewHeader = new GridViewHeader
        {
            HeaderText = string.Concat(new string(' ', listViewOffset), table.Header),
            HeaderUnderLine = string.Concat(new string(' ', listViewOffset), table.HeaderLine)
        };

        var gridViewDataSource = new GridViewDataSource(table.Rows.Select((line, index) => new GridViewRow
        {
            DisplayString = line,
            OriginalIndex = index
        }));

        return (
            gridViewHeader, 
            gridViewDataSource);
    }
}
