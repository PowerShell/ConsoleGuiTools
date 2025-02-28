// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.PowerShell.ConsoleGuiTools.Models;

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Data transfer object containing the formatted table data.
/// </summary>
internal class Table
{
    /// <summary>
    /// Represents an empty table.
    /// </summary>
    internal static Table Empty { get; } = new Table
    {
        Header = string.Empty,
        HeaderLine = string.Empty,
        Rows = Enumerable.Empty<string>()
    };

    /// <summary>
    /// Gets the header of the table with labels for each column in a line.
    /// </summary>
    internal string Header { get; init; }

    /// <summary>
    /// Gets the line under the header with dashes and spaces ('---- -- --- ').
    /// </summary>
    internal string HeaderLine { get; init; }

    /// <summary>
    /// Gets the rows of the table.
    /// </summary>
    internal IEnumerable<string> Rows { get; init; }
}
