// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.PowerShell.ConsoleGuiTools.Models;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

internal class ApplicationData
{
    public string Title { get; set; }
    public OutputModeOption OutputMode { get; set; }
    public bool PassThru { get; set; }
    public string Filter { get; set; }
    public bool MinUI { get; set; }
   
    public bool UseNetDriver { get; set; }
    public bool Verbose { get; set; }
    public bool Debug { get; set; }

    public string ModuleVersion { get; set; }

    /// <summary>
    /// Get's the objects from the pipeline.
    /// </summary>
    public IReadOnlyList<PSObject> Input { get; init; }

    /// <summary>
    /// Gets the Properties parameter.
    /// </summary>
    public object[] Properties { get; init; }

    /// <summary>
    /// Gets the Force parameter.
    /// </summary>
    public bool Force { get; init; }
}