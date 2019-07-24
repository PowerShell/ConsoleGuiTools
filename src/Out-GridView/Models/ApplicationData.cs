using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace OutGridView.Models
{
    public class ApplicationData
    {
        public string Title { get; set; }
        public OutputModeOption OutputMode { get; set; }
        public bool PassThru { get; set; }
        public List<PSObject> Objects { get; set; }
    }
}