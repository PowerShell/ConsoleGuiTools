// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.PowerShell.ConsoleGuiTools;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerShell.ConsoleGuiTools.Models;

/// <summary>
/// Helper class for formatting objects passed pipeline object into text.
/// </summary>
internal static class FormatHelper
{
    /// <summary>
    /// Formats the output of a command as a table with the selected properties of the object in each column. 
    /// The object type determines the default layout and properties that are displayed in each column. 
    /// You can use the Property parameter to select the properties that you want to display.
    /// </summary>
    /// <param name="inputs">Collection of <see cref="PSObject"/></param>
    /// <param name="properties">Specifies the object properties that appear in the display and the order in which they appear. 
    /// Type one or more property names, separated by commas, or use a hash table to display a calculated property.
    /// Wildcards are permitted.</param>
    /// <returns><see cref="Table"/> data transfer object that contains header and rows in string.</returns>
    /// <remarks>
    /// <c>Format-Table</c> Powershell command is used to format the inputs objects as a table.
    /// </remarks>
    internal static Table FormatTable(IReadOnlyList<PSObject> inputs, bool force, object[] properties = null)
    {
        if (inputs.Count == 0)
        {
            return Table.Empty;
        }

        using var ps = PowerShell.Create(RunspaceMode.CurrentRunspace);
        ps.AddCommand("Format-Table");
        ps.AddParameter("AutoSize");

        if (properties != null)
        {
            ps.AddParameter("Property", properties);
        }   
        
        if (force == true)
        {
            ps.AddParameter("Force");
        }

        ps.AddParameter("InputObject", inputs);

        // Format-Table's output objects are internal to Powershell,
        // we cannot use them here,  so we need to convert it to a string as workaround.
        ps.AddCommand("Out-String");

        var results = ps.Invoke();
        var text = results.FirstOrDefault()?.BaseObject.ToString();

        var lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Where(line => string.IsNullOrEmpty(line) == false).ToList();

        /* 
         * Format-Table sometimes outputs a label on the top based on input's data type.
         * We need to detect and skip the label and extract only header and rows.
         * Strategy is to detect the index of the line under the header with dashes and spaces ('---- -- --- ').
        */

        static bool isHeaderLine(string text) => text.Contains('-') && text.All(c => c == '-' || c == ' ');

        var headerLineIndex = lines.FindIndex(isHeaderLine);

        if (headerLineIndex == -1)
        {
            // unexpected result, return the whole text
            headerLineIndex = 1;
        }

        return new Table
        {
            Header = lines.Skip(headerLineIndex - 1).FirstOrDefault(),
            HeaderLine = lines.Skip(headerLineIndex).FirstOrDefault(),
            Rows = lines.Skip(headerLineIndex + 1)
        };
    }
}
