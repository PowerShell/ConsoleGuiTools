using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;

namespace OutGridView
{
    [Cmdlet(VerbsDiagnostic.Test, "GridViewCross")]
    public class OutGridViewCmdletCommand : PSCmdlet
    {

        [Parameter(Position = 1, Mandatory = true, ValueFromPipeline = true)]
        public PSObject inputObject
        {
            get;
            set;
        }
        public List<PSObject> PSObjects
        {
            get;
            set;
        } = new List<PSObject>();

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            WriteVerbose("Begin!");
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            PSObjects.Add(inputObject);

        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {

            AvaloniaAppRunner.RunApp(PSObjects);
            WriteVerbose("End!");
        }
    }
}