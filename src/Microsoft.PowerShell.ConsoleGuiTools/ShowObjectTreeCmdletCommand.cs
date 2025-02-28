// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Internal;

using Microsoft.PowerShell.ConsoleGuiTools.Models;

namespace Microsoft.PowerShell.ConsoleGuiTools;

[Cmdlet("Show", "ObjectTree")]
[Alias("shot")]
public class ShowObjectTreeCmdletCommand : PSCmdlet, IDisposable
{
    #region Properties

    private const string DataNotQualifiedForShowObjectTree = nameof(DataNotQualifiedForShowObjectTree);
    private const string EnvironmentNotSupportedForShowObjectTree = nameof(EnvironmentNotSupportedForShowObjectTree);

    private List<PSObject> _psObjects = new List<PSObject>();

    #endregion Properties

    #region Input Parameters

    /// <summary>
    /// This parameter specifies the current pipeline object.
    /// </summary>
    [Parameter(ValueFromPipeline = true, HelpMessage = "Specifies the input pipeline object")]
    public PSObject InputObject { get; set; } = AutomationNull.Value;

    /// <summary>
    /// Gets/sets the title of the Out-GridView window.
    /// </summary>
    [Parameter(HelpMessage = "Specifies the text that appears in the title bar of the Out-ConsoleGridView window. y default, the title bar displays the command that invokes Out-ConsoleGridView.")]
    [ValidateNotNullOrEmpty]
    public string Title { get; set; }

    /// <summary>
    /// gets or sets the initial value for the filter in the GUI
    /// </summary>
    [Parameter(HelpMessage = "Pre-populates the Filter edit box, allowing filtering to be specified on the command line. The filter uses regular expressions.")]
    public string Filter { set; get; }

    /// <summary>
    /// gets or sets the whether "minimum UI" mode will be enabled
    /// </summary>
    [Parameter(HelpMessage = "If specified no window frame, filter box, or status bar will be displayed in the GUI.")]
    public SwitchParameter MinUI { set; get; }
    /// <summary>
    /// gets or sets the whether the Terminal.Gui System.Net.Console-based ConsoleDriver will be used instead of the
    /// default platform-specific (Windows or Curses) ConsoleDriver.
    /// </summary>
    [Parameter(HelpMessage = "If specified the Terminal.Gui System.Net.Console-based ConsoleDriver (NetDriver) will be used.")]
    public SwitchParameter UseNetDriver { set; get; }

    /// <summary>
    /// For the -Debug switch
    /// </summary>
    public bool Debug => MyInvocation.BoundParameters.TryGetValue("Debug", out var o);

    #endregion Input Parameters

    // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
    protected override void BeginProcessing()
    {
        if (Console.IsInputRedirected)
        {
            ErrorRecord error = new ErrorRecord(
                new PSNotSupportedException("Not supported in this environment (when input is redirected)."),
                EnvironmentNotSupportedForShowObjectTree,
                ErrorCategory.NotImplemented,
                null);

            ThrowTerminatingError(error);
        }
    }

    // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
    protected override void ProcessRecord()
    {
        if (InputObject == null || InputObject == AutomationNull.Value)
        {
            return;
        }

        if (InputObject.BaseObject is IDictionary dictionary)
        {
            // Dictionaries should be enumerated through because the pipeline does not enumerate through them.
            foreach (DictionaryEntry entry in dictionary)
            {
                ProcessObject(PSObject.AsPSObject(entry));
            }
        }
        else
        {
            ProcessObject(InputObject);
        }
    }

    private void ProcessObject(PSObject input)
    {

        object baseObject = input.BaseObject;

        // Throw a terminating error for types that are not supported.
        if (baseObject is ScriptBlock ||
            baseObject is SwitchParameter ||
            baseObject is PSReference ||
            baseObject is PSObject)
        {
            ErrorRecord error = new ErrorRecord(
                new FormatException("Invalid data type for Show-ObjectTree"),
                DataNotQualifiedForShowObjectTree,
                ErrorCategory.InvalidType,
                null);

            ThrowTerminatingError(error);
        }

        _psObjects.Add(input);
    }

    // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
    protected override void EndProcessing()
    {
        base.EndProcessing();

        //Return if no objects
        if (_psObjects.Count == 0)
        {
            return;
        }

        var applicationData = new ApplicationData
        {
            Title = Title ?? "Show-ObjectTree",
            Filter = Filter,
            MinUI = MinUI,
            UseNetDriver = UseNetDriver,
            Debug = Debug,
            ModuleVersion = MyInvocation.MyCommand.Version.ToString()
        };

        ShowObjectView.Run(_psObjects, applicationData);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
