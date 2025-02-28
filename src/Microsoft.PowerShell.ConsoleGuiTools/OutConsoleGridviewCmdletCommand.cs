// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Reflection.Metadata;
using Microsoft.PowerShell.ConsoleGuiTools.Models;

namespace Microsoft.PowerShell.ConsoleGuiTools
{

    [Cmdlet(VerbsData.Out, "ConsoleGridView")]
    [Alias("ocgv")]
    public class OutConsoleGridViewCmdletCommand : PSCmdlet, IDisposable
    {
        #region Properties

        private const string DataNotQualifiedForGridView = nameof(DataNotQualifiedForGridView);
        private const string EnvironmentNotSupportedForGridView = nameof(EnvironmentNotSupportedForGridView);

        private List<PSObject> _psObjects = new List<PSObject>();
        private ConsoleGui _consoleGui = new ConsoleGui();

        #endregion Properties

        #region Input Parameters

        /// <summary>
        /// Positional parameter for properties, property sets and table sets
        /// specified on the command line.
        /// The parameter is optional, since the defaults
        /// will be determined using property sets, etc.
        /// </summary>
        /// <remarks>
        /// This parameter will be passed to <seealso href="https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/format-table">Format-Table</seealso> command.
        /// Help message is copied from Format-Table documentation.
        /// </remarks>
        [Parameter(Position = 0,
            HelpMessage = @"Specifies the object properties that appear in the display and the order in which they appear. Type one or more property names, separated by commas, or use a hash table to display a calculated property. Wildcards are permitted.

If you omit this parameter, the properties that appear in the display depend on the first object's properties. For example, if the first object has PropertyA and PropertyB but subsequent objects have PropertyA, PropertyB, and PropertyC, then only the PropertyA and PropertyB headers are displayed.

The value of the Property parameter can be a new calculated property. The calculated property can be a script block or a hash table.Valid key-value pairs are:
        
    - Name (or Label) - <string>
    - Expression - <string> or <script block>
    - FormatString - <string>
    - Width - <int32> - must be greater than 0
    - Alignment - value can be Left, Center, or Right
        
For more information, see about_Calculated_Properties https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_calculated_properties?view=powershell-7.4"
            )]
        public object[] Property { get; set; }

        /// <summary>
        /// Indicates that the cmdlet directs the cmdlet to display all the error information. Use with the DisplayError or ShowError parameter. By default, when an error object is written to the error or display streams, only some error information is displayed.
        /// </summary>
        /// <remarks>
        /// This parameter will be passed to <seealso href="https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/format-table">Format-Table</seealso> command.
        /// Help message is copied from Format-Table documentation.
        /// </remarks>
        [Parameter(HelpMessage = @"Indicates that the cmdlet directs the cmdlet to display all the error information. Use with the DisplayError or ShowError parameter. 
By default, when an error object is written to the error or display streams, only some error information is displayed.

If you want to use Format-Table with the Property parameter, you need to include the Force parameter under any of the following conditions:

    - The input objects are normally formatted out-of-band using the ToString() method. This applies to [string] and .NET primitive types, which are a superset of the built-in numeric types such as [int], [long], and others.
    - The input objects have no public properties.
    - The input objects are instances of the wrapper types PowerShell uses for output streams other than the Success output stream. This applies only when these wrapper types are sent to the Success output stream that requires either having captured them via common parameters such as ErrorVariable first or using a redirection such as *>&1.")]
        public SwitchParameter Force { get; set; }

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
        /// Get or sets a value indicating whether the selected items should be written to the pipeline
        /// and if it should be possible to select multiple or single list items.
        /// </summary>
        [Parameter(HelpMessage = "Determines whether a single item (Single), multiple items (Multiple; default), or no items (None) will be written to the pipeline. Also determines selection behavior in the GUI.")]
        public OutputModeOption OutputMode { set; get; } = OutputModeOption.Multiple;

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
        /// For the -Verbose switch
        /// </summary>
        public bool Verbose => MyInvocation.BoundParameters.TryGetValue("Verbose", out var o);

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
                    EnvironmentNotSupportedForGridView,
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
                    new FormatException("Invalid data type for Out-ConsoleGridView"),
                    DataNotQualifiedForGridView,
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
                Title = Title ?? "Out-ConsoleGridView",
                OutputMode = OutputMode,
                Filter = Filter,
                MinUI = MinUI,
                UseNetDriver = UseNetDriver,
                Verbose = Verbose,
                Debug = Debug,
                ModuleVersion = MyInvocation.MyCommand.Version.ToString(),
                Input = this._psObjects,
                Properties = this.Property,
                Force = this.Force
            };

            var selectedIndexes = _consoleGui.Start(applicationData);

            foreach (int idx in selectedIndexes)
            {
                var selectedObject = _psObjects[idx];
                if (selectedObject == null)
                {
                    continue;
                }
                WriteObject(selectedObject, false);
            }
        }

        public void Dispose()
        {
            _consoleGui.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}