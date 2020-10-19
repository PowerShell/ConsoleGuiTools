// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Internal;
using OutGridView.Models;

namespace OutGridView.Cmdlet
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
        /// This parameter specifies the current pipeline object.
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; } = AutomationNull.Value;

        /// <summary>
        /// Gets/sets the title of the Out-GridView window.
        /// </summary>
        [Parameter]
        [ValidateNotNullOrEmpty]
        public string Title { get; set; }

        /// <summary>
        /// Get or sets a value indicating whether the selected items should be written to the pipeline
        /// and if it should be possible to select multiple or single list items.
        /// </summary>
        [Parameter()]
        public OutputModeOption OutputMode { set; get; } = OutputModeOption.Multiple;

        /// <summary>
        /// gets or sets the initial value for the filter in the GUI
        /// </summary>
        [Parameter()]
        public string Filter { set; get; }

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
                    new FormatException("Invalid data type for Out-GridView"),
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

            var TG = new TypeGetter(this);

            var dataTable = TG.CastObjectsToTableView(_psObjects);
            var applicationData = new ApplicationData
            {
                Title = Title ?? "Out-ConsoleGridView",
                OutputMode = OutputMode,
                Filter = Filter,
                DataTable = dataTable
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
        }
    }
}
