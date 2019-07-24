
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Internal;
using OutGridView.Models;
using System.Management.Automation.Runspaces;

using Microsoft.PowerShell.Commands.Internal.Format;
namespace OutGridView
{
    /// Enum for SelectionMode parameter.
    /// </summary>
    [Cmdlet(VerbsData.Out, "CrossGridView", DefaultParameterSetName = "PassThru", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=113364")]
    public class OutGridViewCmdletCommand : PSCmdlet
    {
        #region Properties

        private const string DataNotQualifiedForGridView = "DataNotQualifiedForGridView";

        private List<PSObject> PSObjects = new List<PSObject>();

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
        /// Get or sets a value indicating whether the cmdlet should wait for the window to be closed.
        /// </summary>
        [Parameter(ParameterSetName = "Wait")]
        public SwitchParameter Wait { get; set; }

        /// <summary>
        /// Get or sets a value indicating whether the selected items should be written to the pipeline
        /// and if it should be possible to select multiple or single list items.
        /// </summary>
        [Parameter(ParameterSetName = "OutputMode")]
        public OutputModeOption OutputMode { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the selected items should be written to the pipeline.
        /// Setting this to true is the same as setting the OutputMode to Multiple.
        /// </summary>
        [Parameter(ParameterSetName = "PassThru")]
        public SwitchParameter PassThru
        {
            set { this.OutputMode = value.IsPresent ? OutputModeOption.Multiple : OutputModeOption.None; }

            get { return OutputMode == OutputModeOption.Multiple ? new SwitchParameter(true) : new SwitchParameter(false); }
        }

        #endregion Input Parameters

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            WriteVerbose("Begin!");
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            if (InputObject == null || InputObject == AutomationNull.Value)
            {
                return;
            }

            IDictionary dictionary = InputObject.BaseObject as IDictionary;
            if (dictionary != null)
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

        protected override void StopProcessing()
        {
            if (this.Wait || this.OutputMode != OutputModeOption.None)
            {
                AvaloniaAppRunner.CloseWindow();
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
                    new FormatException(""),
                    DataNotQualifiedForGridView,
                    ErrorCategory.InvalidType,
                    null);

                this.ThrowTerminatingError(error);
            }

            PSObjects.Add(input);
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            var applicationData = new ApplicationData
            {
                Title = Title,
                OutputMode = OutputMode,
                PassThru = PassThru,
                Objects = PSObjects
            };

            AvaloniaAppRunner.RunApp(applicationData);

            var selectedObjects = AvaloniaAppRunner.GetPassThruObjects();

            foreach (PSObject selectedObject in selectedObjects)
            {
                if (selectedObject == null)
                {
                    continue;
                }
                this.WriteObject(selectedObject, false);
            }
            WriteVerbose("End!");
        }
    }
}